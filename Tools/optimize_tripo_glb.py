"""Tripo GLB -> game-ready GLB (v2).
Steps: weld UV-seam duplicates -> bake Tripo texture to vertex colors -> optionally drop loose
debris pieces below a height -> quadric decimation -> re-transfer colors from the full-res mesh
-> export GLB (COLOR_0 + simple matte PBR material).
Usage: python optimize_tripo_glb.py in.glb out.glb target_faces [drop_below_y]
Needs: pip install trimesh pymeshlab pillow numpy scipy"""
import sys, json, struct, numpy as np, trimesh, pymeshlab
from scipy.sparse import coo_matrix
from scipy.sparse.csgraph import connected_components

src, dst, target = sys.argv[1], sys.argv[2], int(sys.argv[3])
drop_y = float(sys.argv[4]) if len(sys.argv) > 4 else None

g = list(trimesh.load(src).geometry.values())[0]
tex = np.asarray(g.visual.material.baseColorTexture.convert("RGB"))
H, W, _ = tex.shape
uv = np.asarray(g.visual.uv)
px = (np.mod(uv[:, 0], 1.0) * (W - 1)).astype(int); py = ((1 - np.mod(uv[:, 1], 1.0)) * (H - 1)).astype(int)
col = tex[py, px].astype(np.float64) / 255.0
v, f = np.asarray(g.vertices), np.asarray(g.faces)

# weld vertices that share a position (UV seams)
_, first, inv = np.unique(np.round(v, 6), axis=0, return_index=True, return_inverse=True)
wv, wc, wf = v[first], col[first], inv[f]
wf = wf[(wf[:, 0] != wf[:, 1]) & (wf[:, 1] != wf[:, 2]) & (wf[:, 0] != wf[:, 2])]

if drop_y is not None:  # remove loose pieces whose top is below drop_y (baked-in rubble)
    e = np.concatenate([wf[:, [0, 1]], wf[:, [1, 2]]]); n = len(wv)
    _, lab = connected_components(coo_matrix((np.ones(len(e)), (e[:, 0], e[:, 1])), shape=(n, n)), directed=False)
    fl = lab[wf[:, 0]]; keep = np.ones(len(wf), bool)
    for c in np.unique(fl):
        m = fl == c
        if wv[wf[m]].reshape(-1, 3)[:, 1].max() < drop_y: keep[m] = False
    print("dropped", int((~keep).sum()), "faces of debris below y=", drop_y)
    wf = wf[keep]

rgba = np.c_[wc, np.ones(len(wc))]
ms = pymeshlab.MeshSet()
ms.add_mesh(pymeshlab.Mesh(vertex_matrix=wv, face_matrix=wf, v_color_matrix=rgba), "full")
ms.apply_filter("meshing_remove_unreferenced_vertices")
ms.add_mesh(ms.mesh(0), "low") if False else None
ms.generate_copy_of_current_mesh()           # mesh 1 = working copy
ms.set_current_mesh(1)
ms.meshing_decimation_quadric_edge_collapse(targetfacenum=target, qualitythr=0.6, preservenormal=True, optimalplacement=True, autoclean=True)
ms.transfer_attributes_per_vertex(sourcemesh=0, targetmesh=1, colortransfer=True, upperbound=pymeshlab.PercentageValue(3))
low = ms.mesh(1)
lv, lf, lc = low.vertex_matrix(), low.face_matrix(), low.vertex_color_matrix()
m = trimesh.Trimesh(lv, lf, vertex_colors=(np.clip(lc, 0, 1) * 255).astype(np.uint8), process=False)
m.vertex_normals  # cache smooth normals so they export
m.export(dst)

# patch material: matte, non-metallic (glTF default is metallic=1 which renders black without env map)
d = bytearray(open(dst, "rb").read()); clen = struct.unpack("<I", d[12:16])[0]
j = json.loads(d[20:20 + clen]); bin_chunk = bytes(d[20 + clen:])
j["materials"] = [{"name": "TripoBaked", "pbrMetallicRoughness": {"baseColorFactor": [1, 1, 1, 1], "metallicFactor": 0.0, "roughnessFactor": 0.65}}]
for mesh in j["meshes"]:
    for p in mesh["primitives"]: p["material"] = 0
js = json.dumps(j, separators=(",", ":")).encode(); js += b" " * ((4 - len(js) % 4) % 4)
out = struct.pack("<4sII", b"glTF", 2, 12 + 8 + len(js) + len(bin_chunk)) + struct.pack("<I4s", len(js), b"JSON") + js + bin_chunk
open(dst, "wb").write(out)
import os
print(os.path.basename(src), "->", os.path.basename(dst), len(f), "->", len(m.faces), "faces;", os.path.getsize(dst) // 1024, "KB; bbox", m.bounds.round(3).tolist())

from PIL import Image, ImageDraw, ImageFont
W,H=630,500
im=Image.new("RGB",(W,H),(14,16,26)); d=ImageDraw.Draw(im)
for y in range(H): d.line([(0,y),(W,y)],fill=(14+y//12,16+y//14,26+y//8))
d.polygon([(0,380),(W,380),(W,H),(0,H)],fill=(32,36,52))
for x,h in [(70,180),(170,130),(470,150),(560,200)]: d.rectangle([x,380-h,x+34,380],fill=(80,84,104))
for x,y in [(250,330),(380,350),(310,300)]:
    d.polygon([(x,y-40),(x+22,y),(x,y+30),(x-22,y)],fill=(255,215,60),outline=(255,245,170))
d.ellipse([140,300,200,360],fill=(60,215,255)); d.rectangle([150,350,190,390],fill=(60,215,255))
d.ellipse([450,250,520,320],fill=(230,30,50)); d.ellipse([474,272,496,294],fill=(255,230,230))
try: f=ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",64); g=ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",24)
except: f=g=ImageFont.load_default()
d.text((W//2,90),"CRYSTAL",font=f,fill=(255,215,60),anchor="mm"); d.text((W//2,160),"BREAK",font=f,fill=(60,215,255),anchor="mm")
d.text((W//2,215),"Collect. Dodge. Escape.",font=g,fill=(220,220,235),anchor="mm")
im.save("Docs/cover_placeholder.png")

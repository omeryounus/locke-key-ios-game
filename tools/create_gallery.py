import os
import shutil

src_dir = "/Users/omer/.gemini/antigravity/scratch/locke-key-ios-game/Assets/GrokWireframes"
dest_dir = "/Users/omer/.gemini/antigravity/brain/72635f70-7a05-4224-9485-3fbc913a7455/grok_images"
gallery_path = "/Users/omer/.gemini/antigravity/brain/72635f70-7a05-4224-9485-3fbc913a7455/gallery_grok.md"

os.makedirs(dest_dir, exist_ok=True)

files = sorted([f for f in os.listdir(src_dir) if f.endswith(".jpg")])

markdown_lines = [
    "# Grok Wireframe Gallery",
    "",
    "Below is the index of all 65 wireframe mockup images copied from the Downloads folder. We can use this index to map each image to a specific screen (S0-S8) or UI detail.",
    "",
    "| Image | Filename |",
    "| --- | --- |"
]

for filename in files:
    src_file = os.path.join(src_dir, filename)
    dest_file = os.path.join(dest_dir, filename)
    
    # Copy to artifacts directory
    shutil.copy2(src_file, dest_file)
    
    # Add to markdown gallery using absolute path to artifact copy
    markdown_lines.append(f"| ![{filename}](file://{dest_file}) | `{filename}` |")

with open(gallery_path, "w", encoding="utf-8") as f:
    f.write("\\n".join(markdown_lines))

print(f"Success: Copied {len(files)} files and wrote gallery to {gallery_path}")

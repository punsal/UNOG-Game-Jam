-- Validates every VFX source against the approved palette, canvas size and tags.
-- Run:  aseprite -b --script Tools/Aseprite/validate_vfx_palette.lua
-- Exits with clear ERROR lines; prints OK per file when clean.

local script_dir = debug.getinfo(1, "S").source:match("@?(.*/)") or "./"
local manifest = dofile(script_dir .. "vfx_manifest.lua")
local art_root = script_dir .. "../../Art/VFX/"

local allowed = {}
for _, c in ipairs(manifest.palette) do
  allowed[string.format("%d,%d,%d", c.r, c.g, c.b)] = true
end

local errors = 0

for _, src in ipairs(manifest.sources) do
  local path = art_root .. src.file
  local ok, sprite = pcall(function() return app.open(path) end)
  if not ok or sprite == nil then
    print("ERROR missing source: " .. src.file)
    errors = errors + 1
  else
    if sprite.width ~= src.w or sprite.height ~= src.h then
      print(string.format("ERROR %s: canvas %dx%d, expected %dx%d",
        src.file, sprite.width, sprite.height, src.w, src.h))
      errors = errors + 1
    end

    local found_tags = {}
    for _, tag in ipairs(sprite.tags) do found_tags[tag.name] = true end
    for tag_name, _ in pairs(src.tags) do
      if not found_tags[tag_name] then
        print(string.format("ERROR %s: missing tag '%s'", src.file, tag_name))
        errors = errors + 1
      end
    end

    local img = Image(sprite.spec)
    img:drawSprite(sprite, 1)
    for it in img:pixels() do
      local px = it()
      local a = app.pixelColor.rgbaA(px)
      if a > 0 then
        local key = string.format("%d,%d,%d",
          app.pixelColor.rgbaR(px), app.pixelColor.rgbaG(px), app.pixelColor.rgbaB(px))
        if not allowed[key] then
          print(string.format("ERROR %s: off-palette pixel rgb(%s) at (%d,%d)",
            src.file, key, it.x, it.y))
          errors = errors + 1
          break
        end
      end
    end
    sprite:close()
  end
end

if errors == 0 then
  print("Palette validation OK: all sources clean.")
else
  print(string.format("Palette validation FAILED with %d error(s).", errors))
end

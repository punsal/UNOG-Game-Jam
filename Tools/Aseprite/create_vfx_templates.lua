-- Creates .aseprite VFX templates listed in vfx_manifest.lua.
-- Existing files are NEVER overwritten: manual artwork is preserved.
-- Run:  aseprite -b --script Tools/Aseprite/create_vfx_templates.lua

local script_dir = debug.getinfo(1, "S").source:match("@?(.*/)") or "./"
local manifest = dofile(script_dir .. "vfx_manifest.lua")
local art_root = script_dir .. "../../Art/VFX/"

local function file_exists(path)
  local f = io.open(path, "rb")
  if f then f:close() end
  return f ~= nil
end

local created, skipped = 0, 0

for _, src in ipairs(manifest.sources) do
  local path = art_root .. src.file
  if file_exists(path) then
    print("SKIP (exists, preserving art): " .. src.file)
    skipped = skipped + 1
  else
    local sprite = Sprite(src.w, src.h, ColorMode.INDEXED)
    -- Palette: index 0 transparent, then manifest colors.
    local pal = Palette(#manifest.palette + 1)
    pal:setColor(0, Color{ r = 0, g = 0, b = 0, a = 0 })
    for i, c in ipairs(manifest.palette) do
      pal:setColor(i, Color{ r = c.r, g = c.g, b = c.b })
    end
    sprite:setPalette(pal)

    sprite.layers[1].name = "fx"
    sprite:newLayer().name = "notes"

    -- One frame per tag frame count, tags laid out sequentially.
    local frame_index = 1
    local total = 0
    for _, count in pairs(src.tags) do total = total + count end
    for i = 2, total do sprite:newEmptyFrame(i) end
    for tag_name, count in pairs(src.tags) do
      local tag = sprite:newTag(frame_index, frame_index + count - 1)
      tag.name = tag_name
      frame_index = frame_index + count
    end

    sprite:saveAs(path)
    print("CREATED: " .. src.file)
    created = created + 1
  end
end

print(string.format("Templates done. created=%d skipped=%d", created, skipped))

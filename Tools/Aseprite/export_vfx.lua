-- Exports every VFX source to a PNG sprite sheet + JSON metadata under
-- Assets/Art/VFX/, mirroring the Art/VFX/ folder layout.
-- Run:  aseprite -b --script Tools/Aseprite/export_vfx.lua

local script_dir = debug.getinfo(1, "S").source:match("@?(.*/)") or "./"
local manifest = dofile(script_dir .. "vfx_manifest.lua")
local art_root = script_dir .. "../../Art/VFX/"
local out_root = script_dir .. "../../Assets/Art/VFX/"

local exported, missing = 0, 0

for _, src in ipairs(manifest.sources) do
  local path = art_root .. src.file
  local ok, sprite = pcall(function() return app.open(path) end)
  if not ok or sprite == nil then
    print("SKIP missing source: " .. src.file)
    missing = missing + 1
  else
    local rel = src.file:gsub("%.aseprite$", "")
    app.command.ExportSpriteSheet {
      ui = false,
      askOverwrite = false,
      type = SpriteSheetType.HORIZONTAL,
      textureFilename = out_root .. rel .. ".png",
      dataFilename = out_root .. rel .. ".json",
      dataFormat = SpriteSheetDataFormat.JSON_ARRAY,
      filenameFormat = "{title}_{tag}_{tagframe}",
      listTags = true,
      listLayers = false,
      trim = false,          -- keep exact frame dimensions for PPU math
      extrude = false,
      splitTags = false,
    }
    print("EXPORTED: " .. rel .. ".png")
    exported = exported + 1
    sprite:close()
  end
end

print(string.format("Export done. exported=%d missing=%d", exported, missing))

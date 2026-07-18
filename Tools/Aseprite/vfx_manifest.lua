-- Shared manifest for Last Light VFX sources.
-- Each entry: file (under Art/VFX/), canvas w/h, frame tags with frame counts.
-- Palette: navy #0A0E1A, yellow #FFE854, white #FFFFFF, pink #FF4070,
--          red #E62634, cyan #4DE6FF, purple #9C5AE6, ember #FF8C2E.

return {
  palette = {
    { r = 10, g = 14, b = 26 },    -- navy
    { r = 255, g = 232, b = 84 },  -- yellow
    { r = 255, g = 255, b = 255 }, -- white
    { r = 255, g = 64, b = 112 },  -- pink
    { r = 230, g = 38, b = 52 },   -- red
    { r = 77, g = 230, b = 255 },  -- cyan
    { r = 156, g = 90, b = 230 },  -- purple
    { r = 255, g = 140, b = 46 },  -- ember
  },
  sources = {
    { file = "Light/LastLight_LightParticles.aseprite", w = 8, h = 8,
      tags = { idle = 3, low = 3, critical = 3 } },
    { file = "Damage/LastLight_DamageImpact.aseprite", w = 16, h = 16,
      tags = { impact = 3 } },
    { file = "Sacrifice/LastLight_SacrificeTransfer.aseprite", w = 16, h = 16,
      tags = { pay = 3, enter = 2, exit = 2 } },
    { file = "Sacrifice/LastLight_RiskAcceptance.aseprite", w = 16, h = 16,
      tags = { risk = 3 } },
    { file = "Perception/LastLight_PixelVignette.aseprite", w = 16, h = 16,
      tags = { loop = 2 } },
    { file = "Perception/LastLight_HUDLoss.aseprite", w = 16, h = 16,
      tags = { impact = 2, exit = 2 } },
    { file = "Altars/LastLight_AltarPay.aseprite", w = 16, h = 16,
      tags = { idle = 4, active = 3, recover = 2 } },
    { file = "Altars/LastLight_AltarRisk.aseprite", w = 16, h = 16,
      tags = { idle = 4, warning = 3, active = 3 } },
    { file = "Hazards/LastLight_HazardTelegraph.aseprite", w = 16, h = 16,
      tags = { warning = 3, active = 2, recover = 2 } },
    { file = "Transitions/LastLight_SectionTransition.aseprite", w = 16, h = 16,
      tags = { enter = 2, loop = 2, exit = 2 } },
    { file = "Death/LastLight_DeathReassembly.aseprite", w = 32, h = 32,
      tags = { death = 4, reform = 4 } },
    { file = "Finals/LastLight_FinalTransfer.aseprite", w = 32, h = 32,
      tags = { good_final = 4, medium_final = 4, bad_final = 4 } },
  },
}

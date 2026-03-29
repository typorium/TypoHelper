local LightPoint = {}

LightPoint.name = "Typorium_TypoHelper_Entity_LightPoint"
LightPoint.depth = -999999
LightPoint.texture = "entities/Typorium/TypoHelper/LightPoint/texture"

LightPoint.minimumSize = {8, 8}
LightPoint.maximumSize = {8, 8}
LightPoint.canResize = {false, false}

LightPoint.placements = {
    name = "default",
    data = {
        depth = 1,

        light_color = "FFFFFF",
        light_alpha = 1,
        light_start = 32,
        light_end = 64,

        bloom_alpha = 1,
        bloom_radius = 32,

        remove_player_lightning = true
    }
}

LightPoint.fieldInformation = {
    depth = {
        fieldType = "integer"
    },
    light_color = {
        fieldType = "color"
    },
    light_start = {
        fieldType = "integer"
    },
    light_end = {
        fieldType = "integer"
    }
}

return LightPoint
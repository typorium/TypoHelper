
using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.TypoHelper.Entities;

[CustomEntity("Typorium_TypoHelper_Entity_LightPoint")]
public class LightPoint : Entity
{

    // VertexLight's properties
    Color lightColor;
    float lightAlpha;
    int lightStart;
    int lightEnd;
    VertexLight light;

    // BloomPoint's properties
    float bloomAlpha;
    float bloomRadius;
    BloomPoint bloom;

    // Other properties
    bool removePlayerLightning;

    public LightPoint(EntityData data, Vector2 offset) : base(data.Position + offset)
    {

        Collider = new Circle(4);
        

        // VertexLight
        lightColor = data.HexColor("light_color");
        lightAlpha = data.Float("light_alpha");
        lightStart = data.Int("light_start");
        lightEnd = data.Int("light_end");

        light = new VertexLight(
            lightColor,
            lightAlpha,
            lightStart,
            lightEnd
        );

        Add(light);

        // BloomPoint
        bloomAlpha = data.Float("bloom_alpha");
        bloomRadius = data.Float("bloom_radius");

        bloom = new BloomPoint(
            bloomAlpha,
            bloomRadius
        );

        Add(bloom);

        // Other properties
        removePlayerLightning = data.Bool("remove_player_lightning");

    }


    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        if (!removePlayerLightning)
        {
            return;
        }

        Player player = (scene as Level).Tracker.GetEntity<Player>();
        if (player == null)
        {
            return;
        }

        VertexLight vt = player.Get<VertexLight>();
        if (vt != null)
        {
            vt.Alpha = 0f;
        }

        BloomPoint bp = player.Get<BloomPoint>();
        if (bp != null)
        {
            bp.Alpha = 0f;
        }
    }

    public override void Update()
    {
        base.Update();

        light.Alpha = 1f;
        bloom.Alpha = 1f;

        Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
        if (player == null)
        {
            return;
        }

        if(!CollideCheck(player))
        {
            return;
        }
        float maxdistance = (float)Math.Sqrt(Math.Pow(Height / 2 + player.Height - 1, 2) + Math.Pow(Width / 2 + player.Width - 1, 2));
        float distance = Vector2.Distance(Center, player.Center);
        float alpha = Math.Clamp(distance / maxdistance + 0.4f, 0, 1);
        light.Alpha = alpha;
        bloom.Alpha = alpha;
    }
}
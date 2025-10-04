using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;


namespace Celeste.Mod.TypoHelper.Entities
{

    [CustomEntity("Typorium_TypoHelper_Entity_SodaHoldable")]
    public class SodaHoldable : Actor
    {

        // Appearance
        Sprite sprite;

        float diameter_opacity;

        // Position related
        Vector2 previous_position;

        // Collision
        Collision onCollideH;
        Collision onCollideV;

        int diameter;
        Circle detection_diameter;

        Holdable hold;
        HoldableCollider hitSeeker;

        // States
        Vector2 speed;
        Vector2 prevLiftSpeed;
        
        float noGravityTimer;
        float hardVerticalHitSoundCooldown;
        float swatTimer;

        bool was_dropped;
        bool is_exploding;

        static float explosion_framecount = 5f;
        static float explosion_framedelay = 0.05f;

        // Others
        Level level;


        public SodaHoldable(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {

            // Position
			this.previous_position = data.Position;

            // States
            this.LiftSpeedGraceTime = 0.1f;
            this.noGravityTimer = 0f;
            this.hardVerticalHitSoundCooldown = 0f;

            this.was_dropped = false;
            this.is_exploding = false;


            // Collision
			this.Collider = new Hitbox(8f, 10f, -4f, -5f);

            this.diameter = data.Int("diameter") == 0 ? 32 : data.Int("diameter");
            this.detection_diameter = new Circle( this.diameter, 0, 0);

            this.Collidable = true;

            this.hitSeeker = null;

			this.hold = new Holdable(0.1f);
			this.hold.PickupCollider = new Hitbox(this.Collider.Width + 6, this.Collider.Height + 6, this.Collider.Left - 3, this.Collider.Top - 3);
			this.hold.SlowFall = false;
			this.hold.SlowRun = true;
			this.hold.OnPickup = new Action(this.OnPickup);
			this.hold.OnRelease = new Action<Vector2>(this.OnRelease);
			this.hold.DangerousCheck = new Func<HoldableCollider, bool>(this.Dangerous);
			this.hold.OnHitSeeker = new Action<Seeker>(this.HitSeeker);
			this.hold.OnSwat = new Action<HoldableCollider, int>(this.Swat);
			this.hold.OnHitSpring = new Func<Spring, bool>(this.HitSpring);
			this.hold.OnHitSpinner = new Action<Entity>(this.HitSpinner);
			this.hold.SpeedGetter = () => this.speed;
            this.hold.SpeedSetter = (Vector2 new_speed) => this.speed = new_speed; 
            this.Add(this.hold);

			this.onCollideH = new Collision(this.OnCollideH);
			this.onCollideV = new Collision(this.OnCollideV);

            // Appearance
            this.Depth = 100;

            this.diameter_opacity = 0.5f;

            this.Add(new VertexLight(base.Collider.Center, Color.White, 1f, 32, 64));
            this.Add(new MirrorReflection());

            this.sprite = GFX.SpriteBank.Create("Typorium_TypoHelper_SodaHoldable");
            this.sprite.Scale.X = -1f;
            this.Add(this.sprite);

            // Others
            this.level = null;
			
        }


        public void Die()
        {
            this.RemoveSelf();
        }


        private IEnumerator Explode()
        {
            Audio.Play("event:/Typorium_TypoHelper/entities/SodaHoldable/explosion", this.Position);
            this.sprite.Play("explode");

            this.speed = Vector2.Zero;
            this.is_exploding = true;

            Collider normal_collider = this.Collider;
            this.Collider = detection_diameter;

            Player player = this.level.Tracker.GetEntity<Player>();
            if (player != null)
            {
                if (player.CollideCheck(this))
                {
                    player.ExplodeLaunch(this.Center, false, false);
                }
            }

            foreach (Entity touchswitch in base.Scene.Tracker.GetEntities<TouchSwitch>())
            {
                TouchSwitch touchSwitch = (TouchSwitch)touchswitch;
                if (this.CollideCheck(touchSwitch))
                {
                    touchSwitch.TurnOn();
                }
            }

            this.Collider = normal_collider;
            this.Collidable = false;

            float original_opacity = this.diameter_opacity;
            float waiting_time = Math.Max(explosion_framecount - 1, 0) * explosion_framedelay;
            for(float p = waiting_time ; p >= 0; p -= Engine.DeltaTime)
            {
                this.diameter_opacity = original_opacity * (p / waiting_time);
                yield return null;
            }
            this.RemoveSelf();
            yield return null;
        }


        public override void Added(Scene scene)
        {
            base.Added(scene);

            this.level = this.SceneAs<Level>();
        }


        public bool Dangerous(HoldableCollider holdableCollider)
        {
            return !this.hold.IsHeld && this.speed != Vector2.Zero && this.hitSeeker != holdableCollider;
        }

        public void ExplodeLaunch(Vector2 from)
        {
            if (this.hold.IsHeld)
            {
                return;
            }

            this.speed = (base.Center - from).SafeNormalize(120f);
            SlashFx.Burst(base.Center, this.speed.Angle());
        }


        public void HitSeeker(Seeker seeker)
        {
            if (!this.hold.IsHeld)
            {
                this.speed = (base.Center - seeker.Center).SafeNormalize(120f);
            }
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", this.Position);
        }


        public void HitSpinner(Entity spinner)
        {
            bool flag = (!this.hold.IsHeld && this.speed.Length() < 0.01f && base.LiftSpeed.Length() < 0.01f && (this.previous_position - base.ExactPosition).Length() < 0.01f && base.OnGround(1));
            if (!flag)
            {
                return;
            }

            int num = Math.Sign(this.X - spinner.X);
            if (num == 0)
            {
                num = 1;
            }
            this.speed.X = num * 120f;
            this.speed.Y = -30f;
        }


        public bool HitSpring(Spring spring)
        {
            if (!this.hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && this.speed.Y >= 0f)
                {
                    this.speed.X = this.speed.X * 0.5f;
                    this.speed.Y = -160f;
                    this.noGravityTimer = 0.15f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && this.speed.X <= 0f)
                {
                    base.MoveTowardsY(spring.CenterY + 5f, 4f, null);
                    this.speed.X = 220f;
                    this.speed.Y = -80f;
                    this.noGravityTimer = 0.1f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && this.speed.X >= 0f)
                {
                    base.MoveTowardsY(spring.CenterY + 5f, 4f, null);
                    this.speed.X = -220f;
                    this.speed.Y = -80f;
                    this.noGravityTimer = 0.1f;
                    return true;
                }
            }
            return false;
        }


        public override bool IsRiding(Solid solid)
        {
            return this.speed.Y == 0f && base.IsRiding(solid);
        }


        private void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * (float)Math.Sign(this.speed.X));
            }
            this.speed.X = 0f;

            if (this.was_dropped)
            {
                this.Add(new Coroutine(this.Explode(), true));
            }

        }

        
        private void OnCollideV(CollisionData data)
        {

            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * (float)Math.Sign(this.speed.Y));
            }

            if (this.was_dropped)
            {
                this.Add(new Coroutine(this.Explode(), true));
                return;
            }
            
            if (this.speed.Y > 0f)
            {
                if (this.hardVerticalHitSoundCooldown <= 0f)
                {
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", this.Position, "crystal_velocity", Calc.ClampedMap(this.speed.Y, 0f, 200f, 0f, 1f));
                    this.hardVerticalHitSoundCooldown = 0.5f;
                }
                else
                {
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", this.Position, "crystal_velocity", 0f);
                }
            }
            if (this.speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
            {
                this.speed.Y = this.speed.Y * -0.6f;
                return;
            }
            this.speed.Y = 0f;
        }


        private void OnPickup()
        {
            this.speed = Vector2.Zero;
            this.was_dropped = false;
            this.AddTag(Tags.Persistent);
        }


        private void OnRelease(Vector2 force)
        {
            this.RemoveTag(Tags.Persistent);
            this.was_dropped = true;

            if (force == Vector2.Zero)
            {
                force.Y = 1f;
            }

            else if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }

            this.speed = force * 200f;
            if (this.speed != Vector2.Zero)
            {
                this.noGravityTimer = 0.1f;
            }
        }


        public override void OnSquish(CollisionData data)
        {
            if (!base.TrySquishWiggle(data, 3, 3) && !SaveData.Instance.Assists.Invincible)
            {
                this.Die();
            }
        }


        public override void Render()
        {
            base.Render();

            Draw.Circle(this.Center, this.diameter, Color.White * this.diameter_opacity, (int)(this.diameter / 4));
            

        }


        public void Swat(HoldableCollider hc, int dir)
        {
            if (this.hold.IsHeld && this.hitSeeker == null)
            {
                this.swatTimer = 0.1f;
                this.hitSeeker = hc;
                this.hold.Holder.Swat(dir);
            }
        }


        public override void Update()
        {
            base.Update();


            if (this.is_exploding)
            {
                return;
            }

            if (this.swatTimer > 0f)
            {
                this.swatTimer -= Engine.DeltaTime;
            }
            this.hardVerticalHitSoundCooldown -= Engine.DeltaTime;

            this.Depth = 100;
            if (this.hold.IsHeld)
            {
                this.prevLiftSpeed = Vector2.Zero;
            }
            else
            {
                if (base.OnGround(1))
                {
                    float num;
                    if (!base.OnGround(this.Position + Vector2.UnitX * 3f, 1))
                    {
                        num = 20f;
                    }
                    else if (!base.OnGround(this.Position - Vector2.UnitX * 3f, 1))
                    {
                        num = -20f;
                    }
                    else
                    {
                        num = 0f;
                    }
                    this.speed.X = Calc.Approach(this.speed.X, num, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = base.LiftSpeed;
                    if (liftSpeed == Vector2.Zero && this.prevLiftSpeed != Vector2.Zero)
                    {
                        this.speed = this.prevLiftSpeed;
                        this.prevLiftSpeed = Vector2.Zero;
                        this.speed.Y = Math.Min(this.speed.Y * 0.6f, 0f);
                        if (this.speed.X != 0f && this.speed.Y == 0f)
                        {
                            this.speed.Y = -60f;
                        }
                        if (this.speed.Y < 0f)
                        {
                            this.noGravityTimer = 0.15f;
                        }
                    }
                    else
                    {
                        this.prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && this.speed.Y < 0f)
                        {
                            this.speed.Y = 0f;
                        }
                    }
                }
                else if (this.hold.ShouldHaveGravity)
                {
                    float num2 = 800f;
                    if (Math.Abs(this.speed.Y) <= 30f)
                    {
                        num2 *= 0.5f;
                    }
                    float num3 = 350f;
                    if (this.speed.Y < 0f)
                    {
                        num3 *= 0.5f;
                    }
                    this.speed.X = Calc.Approach(this.speed.X, 0f, num3 * Engine.DeltaTime);
                    if (this.noGravityTimer > 0f)
                    {
                        this.noGravityTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        this.speed.Y = Calc.Approach(this.speed.Y, 200f, num2 * Engine.DeltaTime);
                    }
                }
                this.previous_position = base.ExactPosition;
                base.MoveH(this.speed.X * Engine.DeltaTime, this.onCollideH, null);
                base.MoveV(this.speed.Y * Engine.DeltaTime, this.onCollideV, null);
                if (base.Center.X > (float)this.level.Bounds.Right)
                {
                    base.MoveH(32f * Engine.DeltaTime, null, null);
                    if (base.Left - 8f > (float)this.level.Bounds.Right)
                    {
                        base.RemoveSelf();
                    }
                }
                else if (base.Left < (float)this.level.Bounds.Left)
                {
                    base.Left = (float)this.level.Bounds.Left;
                    this.speed.X = this.speed.X * -0.4f;
                }
                else if (base.Top < (float)(this.level.Bounds.Top - 4))
                {
                    base.Top = (float)(this.level.Bounds.Top + 4);
                    this.speed.Y = 0f;
                }
                else if (base.Bottom > (float)this.level.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
                {
                    base.Bottom = (float)this.level.Bounds.Bottom;
                    this.speed.Y = -300f;
                    Audio.Play("event:/game/general/assist_screenbottom", this.Position);
                }
                else if (base.Top > (float)this.level.Bounds.Bottom)
                {
                    this.Die();
                }
                if (base.X < (float)(this.level.Bounds.Left + 10))
                {
                    base.MoveH(32f * Engine.DeltaTime, null, null);
                }
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                TempleGate templeGate = base.CollideFirst<TempleGate>();
                if (templeGate != null && entity != null)
                {
                    templeGate.Collidable = false;
                    base.MoveH((float)(Math.Sign(entity.X - base.X) * 32) * Engine.DeltaTime, null, null);
                    templeGate.Collidable = true;
                }
            }
            this.hold.CheckAgainstColliders();
            if (this.hitSeeker != null && this.swatTimer <= 0f && !this.hitSeeker.Check(this.hold))
            {
                this.hitSeeker = null;
            }
        }
    }
}
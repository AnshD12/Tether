using UnityEngine;
using System.Collections.Generic;
using System;

public class Beast : MonoBehaviour
{
	public static Beast Create(World world)
	{
		GameObject beastGO = new GameObject("Beast");
		Beast beast = beastGO.AddComponent<Beast>();
		beast.world = world;
		return beast;
	}

	public World world;
	public Player player;

	public FContainer holder;

	public FPNodeLink bodyLink;
	public FSprite bodySprite;
	public SphereCollider bodyCollider;

	public Vector2 bodyVelocity = new Vector2();

	public List<Tentacle>tentacles = new List<Tentacle>();

	public float angle = 0;
	public float targetAngle = 0;

	public FSprite eyeSprite;
		
	public void Init(Player player, Vector2 startPos)
	{
		this.player = player;

		world.entityHolder.AddChild(holder = new FContainer());

		gameObject.transform.position = new Vector3(startPos.x * FPhysics.POINTS_TO_METERS,startPos.y * FPhysics.POINTS_TO_METERS,0);
		gameObject.transform.parent = world.root.transform;

		bodyLink = gameObject.AddComponent<FPNodeLink>();
		bodyLink.Init(holder, false);

		bodySprite = new FSprite("Eye/Evil-Eye_"+player.numString+"_01");
		holder.AddChild(bodySprite);

		eyeSprite = new FSprite("Eye/Eye_" + player.numString);
		eyeSprite.scale = 0.33f;
		holder.AddChild(eyeSprite);
		//holder.alpha = 0.25f;

		InitPhysics();

		holder.ListenForUpdate(HandleUpdate);
		holder.ListenForFixedUpdate(HandleFixedUpdate);

		//AddTentacle(new Vector2(-20.0f, -20.0f), -90.0f);
		//AddTentacle(new Vector2(0.0f, -30.0f), 0.0f);
		//AddTentacle(new Vector2(20.0f, -20.0f), 90.0f);
	}

	void AddTentacle(Vector2 pos, float angle)
	{
		Tentacle tentacle = new Tentacle(world, this, pos, angle);
		tentacles.Add(tentacle);
	}

	public void Destroy()
	{
		UnityEngine.Object.Destroy(gameObject);
		
		holder.RemoveFromContainer();

		foreach (Tentacle tentacle in tentacles)
		{
			tentacle.Destroy();
		}
	}
	
	void InitPhysics()
	{
		Rigidbody rb = gameObject.AddComponent<Rigidbody>();
		rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
		rb.angularDrag = 5.0f;
		rb.mass = 12.0f;
		//rb.mass = 0.0f;
		//rb.drag = BeastConfig.DRAG;
		
		bodyCollider = gameObject.AddComponent<SphereCollider>();
		bodyCollider.radius = 35.0f * FPhysics.POINTS_TO_METERS;
		
		PhysicMaterial mat = new PhysicMaterial();
		mat.bounciness = 0.3f;
		mat.dynamicFriction = 0.1f;
		mat.staticFriction = 0.1f;
		mat.frictionCombine = PhysicMaterialCombine.Maximum;
		collider.material = mat;

		eyeSprite.x = 3.0f;
		eyeSprite.y = 19.0f;
	}

	void OnCollisionEnter(Collision coll)
	{
		Orb orb = coll.collider.gameObject.GetComponent<Orb>();

		if (orb != null)
		{
			if(orb.player == this.player)
			{
				if(!world.isGameOver)
				{
					orb.player.AddScore();
				}
				orb.Destroy();
			}
		}
	}

	void HandleUpdate()
	{

	}

	void HandleFixedUpdate()
	{
		//if (world.isGameOver) return; //you can't play if you lose!

		rigidbody.drag = BeastConfig.DRAG;
		
		Gamepad gamepad = player.gamepad;


		Vector2 movementVector = player.isSpecial ? gamepad.rightStick : gamepad.leftStick;

//		if (movementVector.magnitude > 0.1f)
//		{
//			rigidbody.drag = BeastConfig.DRAG;
//		}
//		else
//		{
//			rigidbody.drag = 5.0f;
//		}

		movementVector *= BeastConfig.MOVE_SPEED * Time.smoothDeltaTime * rigidbody.mass;

		if (movementVector.magnitude > 0.1f)
		{
			//			float targetRotation = -rigidbody.velocity.ToVector2InPoints().GetAngle() + 90.0f;
			//
			//			float delta = RXMath.GetDegreeDelta(targetRotation,rigidbody.transform.rotation.eulerAngles.z);
			//
			//			rigidbody.AddTorque(new Vector3(0,0,delta*0.7f));
			
			
			targetAngle = movementVector.GetAngle() + 90.0f;
		}
		
		angle += RXMath.GetDegreeDelta(angle,targetAngle) / 10.0f;
		
		holder.rotation = angle;

		movementVector *= 2.0f;

//		if (player.isSpecial)
//		{
//			if (gamepad.GetButton(PS3ButtonType.Square))
//			{
//				movementVector *= 2.0f;
//			}
//		}
//		else
//		{
//			if (gamepad.GetButton(PS3ButtonType.X))
//			{
//				movementVector *= 2.0f;
//			}
//		}

		bodyVelocity += movementVector;
		
		rigidbody.AddForce(new Vector3(bodyVelocity.x, bodyVelocity.y, 0.0f), ForceMode.Impulse);
		
		bodyVelocity *= BeastConfig.MOVE_FRICTION;

		if (RXRandom.Float() < 0.99f)
		{
			Vector2 pos = this.transform.position.ToVector2InPoints();

			FParticleDefinition pd = new FParticleDefinition("Particles/SplotchA");

			pd.x = pos.x + RXRandom.Range(-20.0f, 20.0f);
			pd.y = pos.y + RXRandom.Range(-20.0f, 20.0f);

			pd.startColor = player.color.CloneWithNewAlpha(0.1f);
			pd.endColor = player.color.CloneWithNewAlpha(0.0f);

			pd.lifetime = 3.0f;
	
			world.backParticles.AddParticle(pd);
		}
	}

	public Vector2 GetPos()
	{
		return new Vector2(transform.position.x * FPhysics.METERS_TO_POINTS, transform.position.y * FPhysics.METERS_TO_POINTS);
	}
}

public static class BeastConfig
{
	public static float DRAG = 15.0f * 0.5f;
	public static float MOVE_SPEED = 35.0f;
	public static float MOVE_FRICTION = 0.4f;

	static BeastConfig()
	{
		FWatcher.Watch(typeof(BeastConfig));
	}
}



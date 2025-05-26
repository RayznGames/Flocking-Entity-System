using UnityEngine;

public class Boids : MonoBehaviour
{
	private Transform[] boids_pos;
	private Vector3[] boids_vel;

	[SerializeField] GameObject boid_prefab;
	public int boid_num;	

	[Space(5)]
	[Header("Response Parameters")]
	//Bois Parameters
	[Range(0,1)] public float maxSteerForce;
	public float maxSpeed;
	public float minSpeed;
	public float groundHeight;

	[Space(5)]
	[Header("Boid Perception")]
	//Boids Separation 
	public float perceptionRadius;
	public float separationDistance;
	public float groundDistance;

	[Space(5)]
	[Header("Boid Rules Weights")]
	//Boids Adjustment Weights
	public float separationWeight;
	public float alignmentWeight;
	public float cohesionWeight;
	public float boidPositionWeight;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{	
		boids_vel = new Vector3[boid_num];
		boids_pos = new Transform[boid_num];
		for (int i = 0; i < boids_pos.Length; i++)
		{            
			var boid = Instantiate(boid_prefab.gameObject, transform.position + Random.insideUnitSphere * 10 ,Quaternion.identity);
			boids_pos[i] = boid.transform;
		}

	}

	// Update is called once per frame
	void Update()
	{
		UpdateBoids();
	}
	private void FixedUpdate()
	{
		AdjustDirectionVelocity();		
	}
	void AdjustDirectionVelocity() 
	{
		Vector3 centralPos = FindCentralPos(boids_pos);

		//Compute all stuff here per each entity
		for (int i = 0; i< boids_vel.Length; i++)
		{			
			//Apply Repulsion					
			Vector3 totalRepulsion = SteerAway(boids_pos[i].position, boids_pos, i);		
			boids_vel[i] += totalRepulsion * separationWeight;

			//Apply alignment
			Vector3 alignmentSteer = Align(boids_pos[i].position, boids_vel, boids_pos, i) - boids_vel[i];			
			boids_vel[i] += alignmentSteer * alignmentWeight;
			
			//Apply cohesion or finding the central position to each boid with a weight
			boids_vel[i] += GetDirection(boids_pos[i].position, centralPos) * cohesionWeight;

			//Allow you to control were do boids want to go and with how much intensity
			boids_vel[i] += GetDirection(boids_pos[i].position, transform.position).normalized * boidPositionWeight;

			//Avoid ground or obstacels
			boids_vel[i] += AvoidGround(boids_pos[i].position);

			//Clamps boids velocitiy to not exeed maxSpeed
			boids_vel[i] = Vector3.ClampMagnitude(boids_vel[i], maxSpeed);

			//If vel below minSpeed set minspeed
			if (boids_vel[i].magnitude < minSpeed )
			{
				boids_vel[i] = boids_vel[i].normalized * minSpeed;
			}
		}
	}

	Vector3 AvoidGround(Vector3 pos)
	{
		Vector3 force = Vector3.zero;

		var dist = Vector3.Distance(pos, new Vector3(pos.x, groundHeight, pos.z));
		if (dist < groundDistance)
		{
			force = Vector3.up * minSpeed / dist;
			force = Vector3.ClampMagnitude(force, maxSteerForce);
		}
		return force;
	}

	/// <summary>
	/// Sters away from all provided positions with total sum of directions
	/// </summary>
	/// <param name="eval_position"></param>
	/// <param name="positions"></param>
	/// <param name="skipIteration"></param>
	/// <returns></returns>
	Vector3 SteerAway(Vector3 eval_position, Transform[] positions , int skipIteration) 
	{
		Vector3 awayVector = Vector3.zero;
		int count = 0;
		for (int i = 0; i < positions.Length; i++)
		{
			if (i == skipIteration)
			{
				continue;
			}
			var dist = Vector3.Distance(eval_position, positions[i].position);
			if (dist < separationDistance)
			{
				//Sums all the awayVectors in the radius of separation
				awayVector += GetInverseDirection(eval_position, positions[i].position).normalized / dist;
				count++;
			}
		}

		Vector3 separationforce = Vector3.zero;
		if (count != 0)
		{
			separationforce = Vector3.ClampMagnitude(awayVector / count, maxSteerForce);
		}		
		return separationforce; 
	}

	/// <summary>
	/// Returns an average aligned vector of all directions
	/// </summary>
	/// <param name="directions"></param>
	/// <returns></returns>
	Vector3 Align(Vector3 eval_position, Vector3[] velocities, Transform[] positions, int skipIteration)
	{
		Vector3 dirSum = Vector3.zero;
		int count = 0;
		for (int i = 0; i < velocities.Length; i++)
		{
			if (i == skipIteration)
			{
				continue;
			}
			var dist = Vector3.Distance(eval_position, positions[i].position);
			if (dist < perceptionRadius)
			{
				//Sums all the awayVectors in the radius of separation
				dirSum += velocities[i];
				count++;
			}
		}
		Vector3 alignment  = Vector3.zero;
		if (count != 0)
		{
			alignment = dirSum / count;
			alignment = Vector3.ClampMagnitude(alignment, maxSteerForce);
		}
		return alignment;
	}


	//FInds the central position from all the provided positions
	Vector3 FindCentralPos(Transform[] positions)
	{			
		Vector3 averagePos = Vector3.zero;
		for (int b = 0; b < positions.Length; b++)
		{
			averagePos += positions[b].transform.position;
		}
		return averagePos / boids_pos.Length;
	}

	Vector3 GetInverseDirection(Vector3 fromPos, Vector3 toPos)
	{
		Vector3 dir = fromPos - toPos;

		dir = Vector3.ClampMagnitude(dir, 1);
		return dir;
	}

	Vector3 GetDirection(Vector3 fromPos, Vector3 toPos)
	{
		Vector3 dir = toPos - fromPos;

		dir = Vector3.ClampMagnitude(dir, 1);
		return dir;
	}
	
	void UpdateBoids() 
	{
		for (int i = 0; i < boids_pos.Length; i++)
		{
			boids_pos[i].position += boids_vel[i] * Time.deltaTime;
			boids_pos[i].LookAt(boids_pos[i].position + boids_vel[i]);
		}
	}

}

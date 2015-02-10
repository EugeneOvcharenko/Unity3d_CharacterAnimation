using UnityEngine;
using System.Collections;

[System.Serializable]
public enum ECharState
{
	idle_up		= 0,
	idle_right	= 1,
	idle_down	= 2,
	idle_left	= 3,
	walk_up		= 4,
	walk_right	= 5,
	walk_down	= 6,
	walk_left	= 7,
	count		= 8
}

[System.Serializable]
public class CCharState
{
	public	ECharState	state;
	public	Vector2[]	frames;
	public	Vector3		direction;
}

[System.Serializable]
public class CCharAnimState
{
	public	CCharState[]states;
	public	int			curState;
	
	public	Vector3		pos;
	public	float		speed = 1f;
	public	int			currentFrame = 0;
	public	float		frameTime = 30f;
	public	float		curFrameTime = 0f;
	
	public	float		moveTime = 500f;
	public	float		curMoveTime = 0f;
	public 	int			retID = 0;
	public 	float		pdt = 0;
	public	int			pid = 0;
	public	float		life;
	public	float		lifeOver = -1f;
	
	public int GetFrameID()
	{
		Vector2 f_localv = states[ curState ].frames[ currentFrame ];

		int x = (int)(pid / 10);
		int y = pid - x * 10;
		
		x *= 3;
		y *= 4;
		
		x += (int)f_localv.x;
		y += (int)f_localv.y;
		
		int ret = y * 32 + x;
		
		return ret;
	}
	
	public void Move( float dt )
	{
		pos += states[ curState ].direction * dt * speed;
		curMoveTime += dt * speed;
		if ( curMoveTime > moveTime )
		{
			//change state
			curMoveTime = 0f;
			if ( pos.x >=  568 ) { curState = (int)ECharState.walk_up; } else
			if ( pos.x <= -568 ) { curState = (int)ECharState.walk_down; } else	
			if ( pos.z >=  320 ) { curState = (int)ECharState.walk_left; } else
			if ( pos.z <= -320 ) { curState = (int)ECharState.walk_right; } else
			{
				curState = Random.Range( 0, (int)ECharState.count - 0 );
			}
		}
	}
	
	public void Update( float dt )
	{
		dt *= 100f;
		curFrameTime += dt * speed;
		if ( curFrameTime > frameTime )
		{
			currentFrame++;
			
			if ( currentFrame >= states[ curState ].frames.Length )
			{
				currentFrame = 0;				
			}
			curFrameTime = 0;
		}
		
		Move( dt );
	}
}

public class CharacterAnimation : MonoBehaviour
{

	Vector2[] frameA = new Vector2[]
	{
		/*idle_up		= */new Vector2( 0, 0 ),//0
		/*idle_right	= */new Vector2( 1, 0 ),//1,
		/*idle_down		= */new Vector2( 2, 1 ),//5,
		/*idle_left		= */new Vector2( 0, 2 ),//6,
		/*walk_up		= */new Vector2( 2, 0 ),//2,
		/*walk_right	= */new Vector2( 1, 1 ),//4,
		/*walk_down		= */new Vector2( 2, 2 ),//8,
		/*walk_left		= */new Vector2( 0, 1 ) //3
	};
	
	Vector2[] frameB = new Vector2[]
	{
		/*idle_up		= */new Vector2( 0, 0 ),//0,
		/*idle_right	= */new Vector2( 1, 0 ),//1,
		/*idle_down		= */new Vector2( 2, 0 ),//5,
		/*idle_left		= */new Vector2( 0, 2 ),//6,
		/*walk_up		= */new Vector2( 1, 3 ),//10,
		/*walk_right	= */new Vector2( 1, 2 ),//7,
		/*walk_down		= */new Vector2( 2, 3 ),//11,
		/*walk_left		= */new Vector2( 0, 3 ) //9
	};
	
	Vector3[] dirAB;
	
	private	ParticleSystem					particlesSystem;
	private	ParticleSystem.Particle[]		particlesData;
	public	CCharAnimState[]				particlesFrames;
	
	public	CCharState[]					animationStates;
	
	public	float							maxLife = 10000f;
	public	int								particlesEmitCount = 10;
	private	float							particleDeltaLife;
	private	int								currentParticleIndex = 0;
	private	bool							autoCreateStates = true;
	public	int								wChars = 10;
	public	int								hChars = 8;
	
	public void Awake()
	{
		Application.targetFrameRate = 60;
		int pCount = particlesEmitCount;
		particlesSystem	= GetComponent<ParticleSystem>();
		particlesData	= new ParticleSystem.Particle[ pCount ];
		particlesFrames	= new CCharAnimState[ pCount ];

		int i;
		int r;
		
		float s = 1f;
		dirAB = new Vector3[ (int)ECharState.count ];
		dirAB[ (int)ECharState.idle_up ]	= new Vector3( 0, 0, 0 );
		dirAB[ (int)ECharState.idle_right ]	= new Vector3( 0, 0, 0 );
		dirAB[ (int)ECharState.idle_down ]	= new Vector3( 0, 0, 0 );
		dirAB[ (int)ECharState.idle_left ]	= new Vector3( 0, 0, 0 );
		dirAB[ (int)ECharState.walk_up ]	= new Vector3( -s, 0, 0 );
		dirAB[ (int)ECharState.walk_right ]	= new Vector3( 0, 0, s );
		dirAB[ (int)ECharState.walk_down ]	= new Vector3( s, 0, 0 );
		dirAB[ (int)ECharState.walk_left ]	= new Vector3( 0, 0, -s );
		
		if ( autoCreateStates )
		{
			r = (int)ECharState.count;
			animationStates = new CCharState[ r ];
			for ( i = 0; i < r; i++ )
			{
				animationStates[ i ] = new CCharState();
				animationStates[ i ].state	= (ECharState)i;
				animationStates[ i ].frames = new Vector2[ 2 ];
				animationStates[ i ].frames[ 0 ] = frameA[ i ];
				animationStates[ i ].frames[ 1 ] = frameB[ i ];
				animationStates[ i ].direction = dirAB[ i ];
			}
		}
		
		for ( i = 0; i < pCount; i++ )
		{
			r = Random.Range( 0, animationStates.Length );
			particlesFrames[ i ] = new CCharAnimState();
			particlesFrames[ i ].curState = Random.Range( 0, animationStates.Length );
			particlesFrames[ i ].pos = new Vector3( Random.Range( -586, 586 ), Random.value, Random.Range( -320, 320 ) );
			particlesFrames[ i ].speed += Random.value / 5f;
			particlesFrames[ i ].moveTime += Random.value * 50f;
			particlesFrames[ i ].frameTime += Random.value * 5f;
			particlesFrames[ i ].states = animationStates;
			particlesFrames[ i ].pid = Random.Range( 0, hChars ) * wChars + Random.Range( 0, wChars );
		}
	}
	
	void Start () 
	{
		particlesSystem.startLifetime = maxLife;
		particlesSystem.Emit( particlesEmitCount );
		particlesSystem.GetParticles( particlesData );
		particleDeltaLife = (float)maxLife / (float)( 32 * 32 );
	}

	void FixedUpdate () 
	{
		GetParticlesArray();

		int i;
		for ( i = 0; i < Mathf.Min( particlesEmitCount, particlesFrames.Length ); i++ )
		{
			particlesFrames[ i ].Update( Time.fixedDeltaTime );
			SetNextParticle( ref particlesFrames[ i ] );
		}
		
		SetParticlesArray();	
	}
	
	public void SetParticlesCount( int count )
	{
		if( particlesEmitCount < count )
		{
			particlesEmitCount = count - particlesEmitCount + 5;
			particlesSystem.Emit( particlesEmitCount );
			particlesEmitCount = particlesSystem.particleCount;
		}
	}
	
	public void GetParticlesArray()
	{
		particlesSystem.GetParticles( particlesData );
		currentParticleIndex = 0;
	}
	
	public void SetNextParticle( ref CCharAnimState state )
	{
		particlesData[ currentParticleIndex ].position = state.pos;
		state.life = maxLife - ( state.GetFrameID() * particleDeltaLife );
		if ( state.lifeOver >= 0 )
		{
			particlesData[ currentParticleIndex ].lifetime = state.lifeOver;
		}
		else
		{
			particlesData[ currentParticleIndex ].lifetime = state.life;
		}
		
		currentParticleIndex++;
	}
	
	public void SetParticlesArray()
	{
		particlesSystem.SetParticles( particlesData, currentParticleIndex );
	}
}

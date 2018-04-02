using TheEngine;
using TheEngine.TheConsole;

// READ PLS

// The prefab should have an audio emmiter that contains an audio source, or the ship itself at least should have an
// audio source component with the Enemy.bnk as a soundbank. So I guess the prefabs must be redone if we want to have 
// audio for the enemies.
// At the start the source should play the engine fx : audio_source.Play("Play_Enemy_Engine");
// Each update the actua speed of the ship must be sent to wwise so the fx changes according the speed.
// When shooting the source must play the shoot fx : audio_source.Play("Play_Shoot");
// When the ship is destroyed the explosion fx should play : audio_source.Play("Play_Enemy_Explosions");


public class Ai_Starship_Movement {

	TheTransform transform;
	TheGameObject gameobject;
	TheAudioSource audio_source;

	TheGameObject target = null;
	public TheVector3 target_pos = TheVector3.Zero;

	public float min_target_distance = 10f;

	public float separation_max_range = 10f;
	public float separation_force = 20f;

	public float Spd = 50.0f;
	public float minSpd = 5.0f;
	float currSpd = 0f;
	public float acc = 1f;
	public int mov_state = 1;

	TheVector3 spdDir = TheVector3.Zero;

	public TheGameObject laser_spawner_L = null;
	public TheGameObject laser_spawner_R = null;
	TheFactory laser_factory;
	bool laser_spawned_left = false;
	public float laser_speed = 20000.0f;
	public float time_between_lasers = 0.02f;
	TheVector3 spawn_pos = new TheVector3();
	TheVector3 spawn_dir = new TheVector3();
	float timer = 0.0f;

	public float sight_range = 100.0f;
	public float sight_angle = 60.0f;

	public bool shooting = false;
	bool ships_in_scene = false;

	void Start () {
		transform = TheGameObject.Self.GetComponent<TheTransform>();
		gameobject = TheGameObject.Self;
		laser_factory = TheGameObject.Self.GetComponent<TheFactory>();
		audio_source = TheGameObject.Self.GetComponent<TheAudioSource>();
		if(laser_factory != null) laser_factory.StartFactory();
		if(laser_spawner_L == null)
			laser_spawner_L = laser_spawner_R;
		if(laser_spawner_R == null)
			laser_spawner_R = laser_spawner_L;
	
		GetNewTarget();
		if(audio_source != null) {
			audio_source.Play("Play_Enemy_Engine");
			audio_source.SetMyRTPCvalue("Speed",currSpd);
		}
		
		
	}
	
	void Update () {

		TheConsole.Log("Speed");

		if(audio_source != null)
			audio_source.SetMyRTPCvalue("Speed",currSpd);
		if(transform == null)
			return;
		if(currSpd < minSpd)
			currSpd = minSpd;
		if(currSpd > Spd)
			currSpd = Spd;
		switch(mov_state) {
			case 0: // idle
				break;
			case 1: // +acc
				currSpd += acc;
				break;
			case 2: // -acc
				currSpd -= acc;
				break;
		}
		spdDir = transform.ForwardDirection;

		TheConsole.Log("Target");
		if(target != null) {
			TheTransform ttrans = target.GetComponent<TheTransform>();
			if(ttrans != null) {
				// No Slerp No Rotation
				//spdDir = ttrans.GlobalPosition - transform.GlobalPosition;
				spdDir = transform.ForwardDirection;
				// Clunky Rotation
				transform.LookAt(ttrans.GlobalPosition);
				//if(TheVector3.Magnitude(ttrans.GlobalPosition - transform.GlobalPosition) < min_target_distance) {
				//	GetNewTarget();
				//	return;
				//}
			}
		}
		else { // Targeting
			GetNewTarget();
			return;
		}

		TheConsole.Log("Separation");

	// Separation
		TheVector3 separation_vector = TheVector3.Zero;
		foreach(TheGameObject go in TheGameObject.GetSceneGameObjects()) {
			TheVector3 goOffset = go.GetComponent<TheTransform>().GlobalPosition - transform.GlobalPosition;
			if(TheVector3.Magnitude(goOffset) < separation_max_range) {
				separation_vector += -goOffset.Normalized * (TheVector3.Magnitude(goOffset) / separation_max_range);
			}
		}

		transform.LocalPosition += separation_vector.Normalized * separation_force * TheTime.DeltaTime;
		transform.LocalPosition += spdDir.Normalized * currSpd * TheTime.DeltaTime;

				
		TheConsole.Log("Shooting");

		// Shooting
		if(target != null) {
			TheTransform ttrans = target.GetComponent<TheTransform>();
			TheVector3 ship_dir = ttrans.GlobalPosition - transform.GlobalPosition;
				if(TheVector3.Magnitude(ship_dir) < sight_range && TheVector3.AngleBetween(transform.ForwardDirection, ship_dir) < sight_angle/2)
					shooting = true;
				else
					shooting = false;
		}
			
		if(shooting) {
			timer += TheTime.DeltaTime;
			if(timer >= time_between_lasers) {
				if(laser_spawned_left) {
					spawn_pos = laser_spawner_R.GetComponent<TheTransform>().GlobalPosition;
					spawn_dir = laser_spawner_R.GetComponent<TheTransform>().ForwardDirection;
					laser_spawned_left = false;
				}
				else {
					spawn_pos = laser_spawner_L.GetComponent<TheTransform>().GlobalPosition;
					spawn_dir = laser_spawner_L.GetComponent<TheTransform>().ForwardDirection;
					laser_spawned_left = true;
				}
				laser_factory.SetSpawnPosition(spawn_pos);
				TheGameObject laser = laser_factory.Spawn();
				TheVector3 laser_velocity = spawn_dir.Normalized * laser_speed * TheTime.DeltaTime;
				laser.GetComponent<TheRigidBody>().SetLinearVelocity(laser_velocity.x, laser_velocity.y, laser_velocity.z);				

				timer = 0.0f;
			}
		}

	}

	bool isEnemy(string your_tag, string other_tag) {
		return (your_tag == "XWING" || your_tag == "YWING") && (other_tag == "TIEFIGHTER" || other_tag == "LANDCRAFTING") || (other_tag == "XWING" || other_tag == "YWING") && (your_tag == "TIEFIGHTER" || your_tag == "LANDCRAFTING");
	}

	void GetNewTarget() {
    	TheGameObject[] ships_in_scene = new TheGameObject[500];
		//List<TheGameObject> ships_in_scene = new List<TheGameObject>();
        int nship = 0;
        foreach (TheGameObject go in TheGameObject.GetSceneGameObjects())
        {
  	      if(isEnemy(gameobject.tag, go.tag))
          {
   			//ships_in_scene.Add(go);               
			ships_in_scene[nship++] = go;
          }
        }
		int newS = 0;
		foreach(TheGameObject go in ships_in_scene) {
			if(go == null)
				break;
			newS++;
		}
		TheGameObject[] auxList = new TheGameObject[newS];
		for(int i = 0; i < newS; i++) {
			auxList[i] = ships_in_scene[i];
		}
        if(auxList.Length > 0)
        {
			target = auxList[0];
			foreach(TheGameObject ship in auxList) {
				float distA = TheVector3.Magnitude(transform.GlobalPosition - ship.GetComponent<TheTransform>().GlobalPosition);
				float currDistB  = TheVector3.Magnitude(transform.GlobalPosition - target.GetComponent<TheTransform>().GlobalPosition);
				if(currDistB > distA) {
					//if(target != auxTarget && target != null)
					target = ship;
				}
			}
			int rand = (int)TheRandom.RandomRange(0f, auxList.Length);
			target = ships_in_scene[rand];
   	   }
	   else
	   {	
			target = ships_in_scene[1];
	   }
 
		//TheGameObject ret = null;
		//TheGameObject[] scene_gos = TheGameObject.GetSceneGameObjects();
		//int nS = 0;
		//foreach(TheGameObject go in scene_gos) {
		//	if(isEnemy(gameobject.tag, go.tag)) {
		//		nS++;
		//	}
		//}
		//if(nS <= 0)
		//	return null;
		//TheGameObject[] enemy_ships = new TheGameObject[nS];
		//int it = 0;
		//for(int i = 0; i < scene_gos.Length; i++) {
		//	if(isEnemy(gameobject.tag, scene_gos[i].tag))
		//		if(scene_gos[i] is TheGameObject)
		//			enemy_ships[it++] = scene_gos[i];
		//		else
		//			return null;
		//}
		//int rand_n = (int)TheRandom.RandomRange(0, nS);
		//ret = enemy_ships[rand_n];
		//return ret;
	}

	TheVector3 roundingVector(TheVector3 v) {
		TheVector3 nv = TheVector3.Zero;
		nv.x = (v.x >= 0) ? (int)(v.x + 0.5f) : (int)(v.x - 0.5);
		nv.y = (v.y >= 0) ? (int)(v.y + 0.5f) : (int)(v.y - 0.5);
		nv.z = (v.z >= 0) ? (int)(v.z + 0.5f) : (int)(v.z - 0.5);
		return nv;
	}

}
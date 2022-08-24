namespace MoenenVoxel {

using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public class CharacterBehaviour : MonoBehaviour {


	
	#region -------- DAT --------


	[System.Serializable]
	public struct BehaviourSetting {
		//tipo de personajes
		public enum CharacterType {
			_2L2H = 0,
			_2L = 1,
			_4L2H = 2,
			_4L = 3
		}

		public CharacterType characterType;
		[Range(0.01f, 10f)]
		//Velocidad de correr
		public float RunSpeed;
		[Range(0.01f, 10f)]
		//velocidad de caminar
		public float WalkSpeed;
		[Range(0f, 30f)]
		//El poder de brincar
		public int JumpPower;
		[Tooltip("How many times can this character jump before landing. ")]
		[Range(0, 5)]
		public int JumpCount;
		[Range(1f, 40f)]
		public float DashSpeed;
		[Range(0f, 10f)]
		public float DashDistance;
		[Range(0.1f, 2f)]
		public float DashGap;
		public WeaponInfo DefaultHand;



		public bool Is2L {
			get {
				return characterType == CharacterType._2L;
			}
		}

		public bool Is2L2H {
			get {
				return characterType == CharacterType._2L2H;
			}
		}

		public bool Is4L {
			get {
				return characterType == CharacterType._4L;
			}
		}

		public bool Is4L2H {
			get {
				return characterType == CharacterType._4L2H;
			}
		}



		public bool HasHand {
			get {
				return Is2L2H || Is4L2H;
			}
		}

		public bool Has2L {
			get {
				return Is2L || Is2L2H;
			}
		}

		public bool Has4L {
			get {
				return Is4L || Is4L2H;
			}
		}




	}


	[System.Serializable]
	public struct ItemHolder {

		public Transform HeadHolder, ArmLHolder, ArmRHolder, HandLHolder, HandRHolder;
		public Transform BodyHolder, LegLHolder, LegRHolder, FootLHolder, FootRHolder;
		public Transform Leg2LHolder, Leg2RHolder;
		public Transform Foot2LHolder, Foot2RHolder;
		public Transform TailHolder;
		

		public void ClearHand () {
			ClearHandL();
			ClearHandR();
		}


		public void ClearHand (bool left) {
			if (left) {
				ClearHandL();
			} else {
				ClearHandR();
			}
		}


		public void ClearHandL () {
			int len = HandLHolder.childCount;
			for (int i = 0; i < len; i++) {
				Item.DespawnItem(HandLHolder.GetChild(0).GetComponent<Item>());
			}
		}

		public void ClearHandR () {
			int len = HandRHolder.childCount;
			for (int i = 0; i < len; i++) {
				Item.DespawnItem(HandRHolder.GetChild(0).GetComponent<Item>());
			}
		}

	}


	[System.Serializable]
	public struct CharacterModel {

		public Transform HeadModel, ArmLModel, ArmRModel, HandLModel, HandRModel;
		public Transform BodyModel, LegLModel, LegRModel, FootLModel, FootRModel;

		public Transform Leg2LModel, Leg2RModel;
		public Transform Foot2LModel, Foot2RModel;
		public Transform TailModel;

	}


	[System.Serializable]
	public struct CharacterOrgan {

		public Transform Head, Body, ArmL, ArmR, HandL, HandR, LegL, LegR, FootL, FootR, Tail;
		public Transform Leg2L, Leg2R;
		public Transform Foot2L, Foot2R;

	}


	#endregion



	#region -------- VAR --------


	public BehaviourSetting behaviourSetting;
	public ItemHolder itemHolder;
	public CharacterModel characterModel;
	public CharacterOrgan characterOrgan;


	public Item HoldingItemL {
		get {
			if (behaviourSetting.HasHand && itemHolder.HandLHolder && itemHolder.HandLHolder.childCount > 0) {
				return itemHolder.HandLHolder.GetChild(0).GetComponent<Item>();
			} else {
				return null;
			}
		}
	}

	public Item HoldingItemR {
		get {
			if (behaviourSetting.HasHand && itemHolder.HandRHolder && itemHolder.HandRHolder.childCount > 0) {
				return itemHolder.HandRHolder.GetChild(0).GetComponent<Item>();
			} else {
				return null;
			}
		}
	}

	



	private CharacterController Chr {
		get {
			if (!chr) {
				chr = GetComponent<CharacterController>();
			}
			return chr;
		}
	}
	private CharacterController chr = null;

	#region --- Animation Var ---

	private Animator Ani{ 
		get {
			if (!ani) {
				ani = GetComponent<Animator>();
			}
			return ani;
		}
	}
	private Animator ani = null;

	private float AniSpeed {
		get {
			return Ani.GetFloat("Speed");
		}
		set {
			Ani.SetFloat("Speed", value);
		}
	}

	private int AniRunWalkID {
		get {
			return (int)Ani.GetFloat("RunWalkID");
		}
		set {
			Ani.SetFloat("RunWalkID", value);
		}
	}

	private int AniWeaponBehaviourID {
		get {
			return behaviourSetting.HasHand ? (int)Ani.GetFloat("AttackID") : 1;
		}
		set {
			Ani.SetFloat("AttackID", value);
			Ani.SetFloat("MovementID",
				value <= 0 ? 0 :
				value <= 2 ? 1 :
				value <= 5 ? 2 :
				value <= 7 ? 3 :
			4);
		}
	}

	private float AniAttackSpeed1 {
		get {
			return Ani.GetFloat("AttackSpeed1");
		}
		set {
			LogicAttackSpeed1 = Mathf.Max(value, MIN_ATTACK_LOGIC_SPEED);
			Ani.SetFloat("AttackSpeed1", Mathf.Max(value, MIN_ATTACK_ANI_SPEED));
		}
	}

	private float AniAttackSpeed2 {
		get {
			return Ani.GetFloat("AttackSpeed2");
		}
		set {
			LogicAttackSpeed2 = Mathf.Max(value, MIN_ATTACK_LOGIC_SPEED);
			Ani.SetFloat("AttackSpeed2", Mathf.Max(value, MIN_ATTACK_ANI_SPEED));
		}
	}

	private int AniAttackRandom {
		get {
			return (int)Ani.GetFloat("AttackRandom");
		}
		set {
			Ani.SetFloat("AttackRandom", (float)value);
			PrevAniAttackRandom = value;
		}
	}
	
	private bool AniOnGround {
		get {
			return Ani.GetBool("OnGround");
		}
		set {
			Ani.SetBool("OnGround", value);
		}
	}

	private bool Attacking {
		get {
			return AniAttack1 || AniAttack2;
		}
	}

	private bool AniAttack1 {
		get {
			return Ani.GetBool("Attack1");
		}
		set {
			Ani.SetBool("Attack1", value);
		}
	}

	private bool AniAttack2 {
		get {
			return Ani.GetBool("Attack2");
		}
		set {
			Ani.SetBool("Attack2", value);
		}
	}

	private bool AniDash {
		get {
			return Ani.GetBool("Dash");
		}
		set {
			Ani.SetBool("Dash", value);
		}
	}

	#endregion

	private float NextAttackTime = float.MinValue;
	private float NextDashTime = float.MinValue;
	private float LogicAttackSpeed1 = 0.02f;
	private float LogicAttackSpeed2 = 0.02f;
	private int CurrentJumpCount = 0;
	private int PrevAniAttackRandom = 0;
	private bool FixAniParamFlag = true;
	private Vector3 AimVelocity = Vector3.zero;

	private const float SAME_ATTACK_CHANCE = 0.3f;
	private const float MIN_ATTACK_ANI_SPEED = 2.4f;
	private const float MIN_ATTACK_LOGIC_SPEED = 0.1f;


	#endregion



	#region -------- API --------


	/// <summary>
	/// Mover este personaje en el mismo frame. 
	/// Llame en cada cuadro para obtener una acción continua.
	/// El movimiento es impulsado por rigidbody.
	/// </summary>
	/// <param name="dir">dirección de movimiento en el espacio mundial</param>
	/// <param name="speed">velocidad de movimiento.(unidad/s)</param>
	public void Move (Vector2 dir, float speed) {
		if (Attacking || AniDash) {
			return;
		}
		Vector2 v = Vector2.ClampMagnitude(dir, 1f) * speed;
		AimVelocity.x = v.x;
		AimVelocity.z = v.y;
	}
	

	/// <summary>
	// Mover este personaje en el mismo frame. 
	/// Llame en cada cuadro para obtener una acción continua.
	/// El movimiento es impulsado por rigidbody.
	/// </summary>
	/// <param name="dir">dirección de movimiento relativa a la cámara principal</param>
	/// <param name="speed">velocidad de movimiento.(unidad/s)</param>
	public void MoveDependCamera (Vector2 dir, float speed) {
		if (Attacking || AniDash) {
			return;
		}
		Vector3 v = Camera.main.transform.up * dir.y + Camera.main.transform.forward * dir.y + Camera.main.transform.right * dir.x;
		v.y = 0f;
		v.Normalize();
		v *= speed;
		AimVelocity.x = v.x;
		AimVelocity.z = v.z;
	}


	/// <summary>
	/// Detener al personaje inmediatamente
	/// </summary>
	public void Stop () {
		Move(Vector2.zero, 0f);
	}


	/// <summary>
	/// Mueve este personaje a su velocidad de carrera.
	/// </summary>
	/// <param name="dir">dirección de movimiento en el espacio mundial</param>
	public void Run (Vector2 dir) {
		Move(dir, behaviourSetting.RunSpeed);
	}


	/// <summary>
	/// Mueve este personaje a su velocidad de carrera.
	/// </summary>
	/// <param name="dir">dirección de movimiento relativa a la cámara principal</param>
	public void RunDependCamera (Vector2 dir) {
		MoveDependCamera(dir, behaviourSetting.RunSpeed);
	}


	/// <summary>
	/// Mueve este personaje a su velocidad de caminata.
	/// </summary>
	/// <param name="dir">dirección de movimiento en el espacio mundial</param>
	public void Walk (Vector2 dir) {
		Move(dir, behaviourSetting.WalkSpeed);
	}


	/// <summary>	
/// Mueve este personaje a su velocidad de caminata.
	/// </summary>
	/// <param name="dir">dirección de movimiento relativa a la cámara principa</param>
	public void WalkDependCamera (Vector2 dir) {
		MoveDependCamera(dir, behaviourSetting.WalkSpeed);
	}


	/// <summary>
/// Gira este carácter con el cuaternión dado.
	/// </summary>
	/// <param name="q">Rotar cuaternión en el espacio mundial</param>
	public void Rotate (Quaternion q) {
		Vector3 v = q.eulerAngles;
		transform.rotation = Quaternion.Euler(0f, v.y, 0f);
		if (AniDash) {
			Vector3 dash = transform.rotation * Vector3.back * behaviourSetting.DashSpeed;
			AimVelocity.x = dash.x;
			AimVelocity.z = dash.z;
			return;
		}
	}


	/// <summary>
/// Gira este carácter con el cuaternión dado.
	/// </summary>
	/// <param name="q">Girar el cuaternión relativo a la MainCamera</param>
	public void RotateDependCamera (Quaternion q) {
		Rotate(q * Camera.main.transform.rotation);
	}


	/// <summary>
/// Deja que este personaje salte una vez.
	/// </summary>
	/// <param name="power">La velocidad en el comienzo del salto..</param>
	public void Jump (float power) {
		BreakAttack();
		if (CurrentJumpCount < behaviourSetting.JumpCount) {
			transform.Translate(0f, 0.01f, 0f);
			AimVelocity.y = Mathf.Clamp01(power) * (float)behaviourSetting.JumpPower;
			CurrentJumpCount++;
		}
	}


	/// <summary>
/// Atajo para Jump(1f).
	/// </summary>
	public void Jump () {
		Jump(1f);
	}


	/// <summary>
/// Pon un objeto en la mano del personaje.
	/// </summary>
	/// <param name="item">el objeto</param>
	/// <param name="leftHand">En la mano izquierda o no</param>
	public void SetItem (Item item, bool leftHand) {
		if (!behaviourSetting.HasHand) {
			return;
		}
		Transform holder = leftHand ? itemHolder.HandLHolder : itemHolder.HandRHolder;
		if (holder != null) {
			if (item) {
				if (item.ItemBehaviourID == ItemBehaviour.TwoHands) {
					item.transform.SetParent(null);
					itemHolder.ClearHand();
					item.transform.SetParent(itemHolder.HandLHolder);
					item.transform.localPosition = Vector3.zero;
					item.transform.localRotation = Quaternion.identity;
					item.transform.localScale = Vector3.one;
				} else {
					item.transform.SetParent(null);
					itemHolder.ClearHand(leftHand);
					item.transform.SetParent(holder);
					item.transform.localPosition = Vector3.zero;
					item.transform.localRotation = Quaternion.identity;
					item.transform.localScale = Vector3.one;
				}
			} else {
				itemHolder.ClearHand(leftHand);
			}
		}
	}



	/// <summary>
	/// Poner un objetos en las manos de un jugador..
	/// </summary>
	/// <param name="item">TEl objeto</param>
	/// <param name="leftHand">En la mano izquierda o no</param>
	public void SetItem (Transform tf, bool leftHand) {
		SetItem(tf ? tf.GetComponent<Item>() : null, leftHand);
	}



	/// <summary>
	///Poner un arma en la mano del personaje
	/// </summary>
	/// <param name="weapon">El arma</param>
	public void SetItem (Weapon weapon) {
		if (!behaviourSetting.HasHand) {
			return;
		}
		if (weapon) {
			if (weapon.WeaponInfo.Type == WeaponType.Shield) {
				SetItem(weapon, false);
				Item l = HoldingItemL;
				if (l && l is Weapon) {
					if (((Weapon)l).ItemBehaviourID != ItemBehaviour.Single) {
						SetItem((Item)null, true);
					}
				} else {
					SetWeaponInfo(behaviourSetting.DefaultHand);
				}
			} else {
				SetItem(weapon, true);
				if(weapon.ItemBehaviourID == ItemBehaviour.Double){
					SetItem(Item.SpawnItem(weapon), false);
				} else if (weapon.ItemBehaviourID == ItemBehaviour.Single) {
					Item r = HoldingItemR;
					if (r && r is Weapon && ((Weapon)r).WeaponInfo.Type != WeaponType.Shield) {
						SetItem((Item)null, false);
					}
				} else {
					SetItem((Item)null, false);
				}
				SetWeaponInfo(weapon);
			}
			
		} else {
			SetWeaponInfo();
			SetItem((Item)null, true);
			SetItem((Item)null, false);
		}
	}



	/// <summary>
	/// Dejar el personaje ataque una vez
	/// </summary>
	/// <param name="id">1 o 2 correspondientes a dos tipos de ataques</param>
	/// <returns></returns>
	public bool Attack (int id = 1) {

		if (Attacking || AniDash || Time.time < NextAttackTime || AniWeaponBehaviourID == 0) {
			return false;
		}

		if (AniOnGround) {
			Stop();
		}
		
		
		AniAttackRandom = Random.value < SAME_ATTACK_CHANCE ? PrevAniAttackRandom : (PrevAniAttackRandom + 1) % 2;
		if (id == 1) {
			AniAttack1 = true;
			NextAttackTime = Time.time + 1f / LogicAttackSpeed1;
			CancelInvoke();
			Invoke("BreakAttack", 1f / AniAttackSpeed1);
			if (HoldingItemL && HoldingItemL is Weapon) {
				((Weapon)HoldingItemL).OnAttack1(transform);
			}
			if (HoldingItemR && HoldingItemR is Weapon) {
				((Weapon)HoldingItemR).OnAttack1(transform);
			}
		} else {
			AniAttack2 = true;
			NextAttackTime = Time.time + 1f / LogicAttackSpeed2;
			CancelInvoke();
			Invoke("BreakAttack", 1f / AniAttackSpeed2);
			if (HoldingItemL && HoldingItemL is Weapon) {
				((Weapon)HoldingItemL).OnAttack2(transform);
			}
			if (HoldingItemR && HoldingItemR is Weapon) {
				((Weapon)HoldingItemR).OnAttack2(transform);
			}
		}
		return true;
	}


	/// <summary>
	/// Atajo para Attack(1).
	/// </summary>
	/// <returns></returns>
	public bool Attack1 () {
		return Attack(1);
	}


	/// <summary>
		/// Atajo para Attack(2).
	/// </summary>
	/// <returns></returns>
	public bool Attack2 () {
		return Attack(2);
	}


	/// <summary>
	/// Parar de atacar inmediantamente
	/// </summary>
	public void BreakAttack () {
		if (Attacking) {
			CancelInvoke();
			BreakDash();
			AniAttack1 = false;
			AniAttack2 = false;
			if (HoldingItemL && HoldingItemL is Weapon) {
				((Weapon)HoldingItemL).StopAttack(transform);
			}
			if (HoldingItemR && HoldingItemR is Weapon) {
				((Weapon)HoldingItemR).StopAttack(transform);
			}
		}
	}


	/// <summary>
/// Deja que el personaje corra una vez. Romperá el ataque.
	/// </summary>
	public void Dash () {
		BreakAttack();
		if (!AniDash && Time.time > NextDashTime) {
			AniDash = true;
			Stop();
			CancelInvoke();
			Invoke("BreakDash", behaviourSetting.DashDistance / behaviourSetting.DashSpeed);
			NextDashTime = Time.time + behaviourSetting.DashDistance / behaviourSetting.DashSpeed + behaviourSetting.DashGap;
			Vector3 dash = transform.rotation * Vector3.back * behaviourSetting.DashSpeed;
			AimVelocity.x = dash.x;
			AimVelocity.z = dash.z;
		}
	}


	/// <summary>
/// Detenga el guión inmediatamente.
	/// </summary>
	public void BreakDash () {
		if (AniDash) {
			CancelInvoke();
			BreakAttack();
			AniDash = false;
			Stop();
		}
	}


	/// <summary>
/// Devuelve si este personaje está en el suelo o no.
	/// </summary>
	/// <returns></returns>
	public bool IsGrounded () {
		return AniOnGround;
	}


	/// <summary>
/// Obtener el estado actual de correr/caminar
	/// </summary>
	public bool RunIfMove {
		get {
			return AniRunWalkID == 1;
		}
		set {
			AniRunWalkID = value ? 1 : 0;
		}
	}

	
	#endregion



	#region -------- MSG --------

	

	protected virtual void Awake () {
		Item item = HoldingItemL ?? HoldingItemR;
		if (behaviourSetting.HasHand) {
			if (item && item is Weapon) {
				SetItem((Weapon)item);
			} else {
				SetItem(HoldingItemL, true);
				SetItem(HoldingItemR, false);
			}
		} else {
			SetWeaponInfo(behaviourSetting.DefaultHand);
		}
	}

   //FUNCION DEL AWAKE
	protected virtual void Update () {

		AniSpeed = new Vector2(AimVelocity.x, AimVelocity.z).magnitude / behaviourSetting.RunSpeed;
		
		if (behaviourSetting.HasHand && FixAniParamFlag) {
			FixAniParamFlag = false;
			Item item = HoldingItemL ?? HoldingItemR;
			if (item && item is Weapon) {
				SetWeaponInfo((Weapon)item);
			} else {
				SetWeaponInfo();
			}
		}

		Chr.Move(AimVelocity * Time.deltaTime);
		bool onGround = Chr.isGrounded;
		if (onGround) {
			CurrentJumpCount = 0;
		}
		AimVelocity.y += onGround ? 0f : Physics.gravity.y * 10f * Time.deltaTime;
		AniOnGround = onGround;
	}

    ///GOLPE DEL COLISIONADOR DEL CONTROL
	protected virtual void OnControllerColliderHit (ControllerColliderHit hit) {
		Rigidbody body = hit.collider.attachedRigidbody;
		if (body == null || body.isKinematic)
			return;
		if (hit.moveDirection.y < -0.3F)
			return;
		Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
		body.velocity = pushDir;
	}


	#endregion



	#region -------- LGC --------
     
	 ///FUNCION DE LA INFORMACION DEL ARMA CARACTEWRISTICA COMO SU VELOCIDAD COMPORTAMIENTO DE ARMA, ETC...

	private void SetWeaponInfo(WeaponBehaviour id, float minD, float maxD, float speed1, float speed2, float range){
		if (behaviourSetting.HasHand) {
			AniWeaponBehaviourID = (int)id;
		}
		AniAttackSpeed1 = speed1;
		AniAttackSpeed2 = speed2;
	}


	private void SetWeaponInfo (WeaponInfo info) {
		SetWeaponInfo(
			info.WeaponBehaviour,
			info.MinDamage,
			info.MaxDamage,
			info.AttackSpeed1,
			info.AttackSpeed2,
			info.AttackRange
		);
	}


	private void SetWeaponInfo (Weapon w) {
		SetWeaponInfo(w.WeaponInfo);
	}


	private void SetWeaponInfo () {
		SetWeaponInfo(WeaponBehaviour.None, 0, 0, 0, 0, 0);
	}


	#endregion



}
}

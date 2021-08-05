using UnityEngine;

namespace UI
{
	public class MainMenu : MonoBehaviour {

		Vector3 DEFAULTSCALE = new Vector3 (400, 400, 400);
		Transform activeCharacter;

		public Transform currentActiveCharacterParent;
	
		void OnEnable () {
			showActivePlayer ();
		}

		void showActivePlayer(){
			if (activeCharacter != null)
				Destroy (activeCharacter.gameObject);
			activeCharacter = Instantiate (UIManager.instance.CurrentActiveCharacter) as Transform;
			activeCharacter.SetParent (currentActiveCharacterParent);
			activeCharacter.localScale = DEFAULTSCALE;
			activeCharacter.localPosition = new Vector3 (0, 0, -250);
			activeCharacter.localRotation = Quaternion.identity;
			activeCharacter.name = UIManager.instance.CurrentActiveCharacter.name;
		}

		void Update(){
			if(activeCharacter != null)
				activeCharacter.Rotate (Vector3.up, Time.deltaTime * 50);
		}

		public void changeCharacterButttonClick(){
			UIManager.instance.CurrState = UIManager.State.CharacterSelect;
		}
	}
}

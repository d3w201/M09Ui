using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class CustomCharacterSelectMenu : MonoBehaviour
    {
        #region Props

        public int currentItemIndex;
        private int _totalCharacters;
        private float _y;
        private float _z;
        private Transform _parent;
        private Transform _activeCharacterParent;
        public Transform currentActiveCharacter;
        private Transform[] _selectableCharacterArray;
        private Button _selectButton;
        public int totalcharacters = 3;
        public int adjacentdistance = 1000;
        public float duration = 0.5f;
        public int rotationspeed = 50;
        public float highlightedscalefactor = 3;
        public Vector3 defaultscale = new Vector3(100, 100, 100);
        Vector3 _highlightedscale;
        private Vector2 _move;
        private bool _canMove;
        private int _rightItemIndex;
        private int _leftItemIndex;
        private NavigationList<Transform> _selectableCharacterList;

        #endregion

        #region Init

        //Init
        private void Awake()
        {
            SetupReferences();
            _highlightedscale = defaultscale * highlightedscalefactor;
            var localPosition = _parent.localPosition;
            _y = localPosition.y;
            _z = localPosition.z;
            LoadCharacters();
            _canMove = true;
        }

        private void SetupReferences()
        {
            Transform[] gameObjectArray = GetComponentsInChildren<Transform>(true);
            foreach (Transform go in gameObjectArray)
                if (go.name.Equals("Parent"))
                    _parent = go;
                else if (go.name.Equals("CurrentActivePlayerParent"))
                    _activeCharacterParent = go;
                else if (go.name.Equals("CharacterName"))
                    CharacterNameText = go.GetComponent<Text>();
                else if (go.name.Equals("Select"))
                    _selectButton = go.GetComponent<Button>();
        }

        private void LoadCharacters()
        {
            SpawnAllCharacters();
            UpdateHighlightedCharacterInfo();

            _totalCharacters = _selectableCharacterArray.Length;

            if (_totalCharacters == 0)
                Debug.LogError("No characters found!");
        }

        private void SpawnAllCharacters()
        {
            for (var i = 0; i < totalcharacters; i++)
            {
                var obj = Instantiate(Resources.Load("Prefabs/Players/Cube" + (i + 1))) as GameObject;
                if (obj is null) continue;
                var character = obj.transform;
                character.SetParent(_parent);
                character.localScale = defaultscale;
                character.localPosition = new Vector3(i * adjacentdistance, 0, -250);
                character.localRotation = Quaternion.identity;
                character.name = "Character " + (i + 1);
                if (character.name.Equals(UIManager.instance.CurrentActiveCharacter.name))
                {
                    SetActiveCharacter(character);
                }

                _selectableCharacterArray ??= new Transform[totalcharacters];
                _selectableCharacterArray[i] = character;
                _selectableCharacterList ??= new NavigationList<Transform>();
                _selectableCharacterList.Add(character);
            }
        }

        void OnEnable()
        {
            //Reset to first element
            Time.timeScale = 1f;
            _parent.localPosition = new Vector3(0, _y, _z);
            currentItemIndex = Mathf.Abs(Mathf.RoundToInt(_parent.localPosition.x / adjacentdistance));
        }

        #endregion

        #region Move

        //onMove
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
            if (_canMove)
            {
                if (_move.Equals(new Vector2(1, 0)))
                {
                    //right.
                    //RightSwipe();
                    StartCoroutine("Lerp",1);
                }
                else if (_move.Equals(new Vector2(-1, 0)))
                {
                    //left.
                    //LeftSwipe();
                    StartCoroutine("Lerp",-1);
                }
            }
        }
        
        private IEnumerator Lerp(int direction)
        {
            Debug.Log("lerping ..." + direction);
            //_canMove = false;
            float timeElapsed = 0;
            if (direction > 0)
                SetActiveCharacter(_selectableCharacterList.MoveNext);
            else
                SetActiveCharacter(_selectableCharacterList.MovePrevious);
            var nextItemPosition = -_selectableCharacterList.Current.localPosition.x;
            currentItemIndex = _selectableCharacterList.CurrentIndex;

            while (timeElapsed < duration)
            {
                _parent.localPosition = Vector3.Lerp(
                    _parent.localPosition,
                    new Vector3(nextItemPosition, _y, _z),
                    timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            _parent.localPosition = new Vector3(nextItemPosition, _y, _z);
            //_canMove = true;
            Debug.Log("end(lerpRight)");
        }

        private void MoveInput(Vector2 newMoveDirection)
        {
            _move = newMoveDirection;
        }

        #endregion

        #region Private

        private void SetActiveCharacter(Transform trnsfrm)
        {
            if (currentActiveCharacter != null)
                Destroy(currentActiveCharacter.gameObject);

            currentActiveCharacter = Instantiate(trnsfrm, _activeCharacterParent, true);
            currentActiveCharacter.localScale = defaultscale;
            currentActiveCharacter.localPosition = new Vector3(0, 0, -250);
            currentActiveCharacter.localRotation = Quaternion.identity;
            currentActiveCharacter.name = trnsfrm.name;

            UpdateSelectButtonState();
        }

        private void UpdateSelectButtonState()
        {
            _selectButton.interactable = !(CharacterNameText.text.Equals(currentActiveCharacter.name));
        }

        void UpdateHighlightedCharacterInfo()
        {
            CharacterNameText.text = _selectableCharacterArray[currentItemIndex].transform.name;
            UpdateSelectButtonState();
        }

        private void Update()
        {
            //highlight selected item 
            _selectableCharacterList.Current.Rotate (Vector3.up, Time.deltaTime * rotationspeed);
            _selectableCharacterList.Current.localScale = _highlightedscale;
            /*_selectableCharacterArray [currentItemIndex].localScale = Vector3.Lerp (
                _selectableCharacterArray [currentItemIndex].localScale, 
                _highlightedscale, 
                Time.deltaTime * rotationspeed
            );*/
        }

        void LeftSwipe()
        {
            //Scale DOWN leftItem, Scale UP rightItem
            if (_leftItemIndex >= 0 && _leftItemIndex < _totalCharacters)
                _selectableCharacterArray[_leftItemIndex].localScale = defaultscale;
            if (_rightItemIndex >= 0 && _rightItemIndex < _totalCharacters)
                _selectableCharacterArray[_rightItemIndex].localScale = defaultscale;
        }

        void RightSwipe()
        {
            //Scale DOWN rightItem, Scale UP leftItem
            if (_leftItemIndex >= 0 && _leftItemIndex < _totalCharacters)
                _selectableCharacterArray[_leftItemIndex].localScale = defaultscale;
            if (_rightItemIndex >= 0 && _rightItemIndex < _totalCharacters)
                _selectableCharacterArray[_rightItemIndex].localScale = defaultscale;
        }

        private Text CharacterNameText { get; set; }

        public void BackButtonClick()
        {
            UIManager.instance.CurrState = UIManager.State.MainMenu;
        }

        #endregion
        
        // . t e s t i n g . a w a i t . \\
        /*private async void AsyncWaitForInput()
        {
            Debug.Log("AsyncWaitForInput");
            var value = await AsyncGetInput();
            Debug.Log(value);
        }

        private async Task<String> AsyncGetInput()
        {
            Debug.Log("AsyncGetInput");
            while (!ready)
            {
                await Task.Yield();
            }
            return "r e t u r n e d a s y n c";
        }*/
        
        private class NavigationList<T> : List<T>
        {
            private int _currentIndex = 0;
            public int CurrentIndex
            {
                get
                {
                    if (_currentIndex > Count - 1) { _currentIndex = 0; }
                    if (_currentIndex < 0) { _currentIndex = Count - 1; }
                    return _currentIndex;
                }
                set { _currentIndex = value; }
            }

            public T MoveNext
            {
                get { _currentIndex++; return this[CurrentIndex]; }
            }

            public T MovePrevious
            {
                get { _currentIndex--; return this[CurrentIndex]; }
            }

            public T Current
            {
                get { return this[CurrentIndex]; }
            }
        }
    }
}
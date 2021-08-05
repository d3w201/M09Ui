using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class CharacterSelectMenu : MonoBehaviour
    {
        #region properties

        const int ONDRAGSCALESPEED = 8;

        //
        const float MOVEBY = 5000f;

        //
        enum DIRECTION
        {
            None,
            Left,
            Right
        };

        //
        DIRECTION currentDirection;

        DIRECTION oldDirection;

        //
        bool resetCharacterRotation;
        bool isDragging;
        bool isScreenTouched;

        bool directionChanged;

        //
        public int currentItemIndex;
        int rightItemIndex;
        int middleItemIndex;
        int leftItemIndex;

        int totalCharacters;

        //
        float swipeDistance;
        float normalizedSwipeDistance;
        float Y;

        float Z;

        //
        Vector2 firstTouchPosition;

        Vector2 lastTouchPosition;

        //
        Vector3 HIGHLIGHTEDSCALE;

        //
        Text characterNameText;

        //
        Transform Parent;
        Transform activeCharacterParent;

        Transform currentActiveCharacter;

        //
        Transform[] selectableCharacterArray;

        //
        Button selectButton;

        //
        /**
	 ******************************************************************************************
	 * YOU CAN CHANGE ALL OF THE BELOW PUBLIC VARIABLES TO CONSTs [Is why they are named all caps]
	 * AFTER YOU ARE DONE TWEAKING AS PER YOUR REQUIREMENTS.
	 ****************************************************************************************** 
	 */
        //
        [Tooltip("Total characters you want to display in the menu.")]
        public int TOTALCHARACTERS = 3;

        //
        [Tooltip("Distance between two adjacent characters.")]
        public int ADJACENTDISTANCE = 1000;

        //
        public Vector3 DEFAULTSCALE = new Vector3(100, 100, 100);

        //
        [Tooltip("Scale factor of current highlighted character x times normal scale.")]
        public float HIGHLIGHTEDSCALEFACTOR = 3;

        //
        [Tooltip(
            "Minimum finger/mouse swipe distance(Normalized between 0 -> 1) across screen width which is to be registered as a swipe.")]
        public float SWIPETHRESHOLD = 0.1f;

        //
        [Tooltip("Speed of spin of the currently highlighted character.")]
        public int ROTATIONSPEED = 50;

        public Vector2 move;
        //

        #endregion

        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        private void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        void Awake()
        {
            setupReferences();

            HIGHLIGHTEDSCALE = DEFAULTSCALE * HIGHLIGHTEDSCALEFACTOR;

            Y = Parent.localPosition.y;
            Z = Parent.localPosition.z;

            loadCharacters();
        }

        void setupReferences()
        {
            Transform[] GameObjectArray = GetComponentsInChildren<Transform>(true);
            foreach (Transform go in GameObjectArray)
                if (go.name.Equals("Parent"))
                    Parent = go;
                else if (go.name.Equals("CurrentActivePlayerParent"))
                    activeCharacterParent = go;
                else if (go.name.Equals("CharacterName"))
                    CharacterNameText = go.GetComponent<Text>();
                else if (go.name.Equals("Select"))
                    selectButton = go.GetComponent<Button>();
        }

        void loadCharacters()
        {
            spawnAllCharacters();
            updateHighlightedCharacterInfo();

            totalCharacters = selectableCharacterArray.Length;

            if (totalCharacters == 0)
                Debug.LogError("No characters found!");
        }

        void spawnAllCharacters()
        {
            for (int i = 0; i < TOTALCHARACTERS; i++)
            {
                GameObject obj = Instantiate(Resources.Load("Prefabs/Players/Cube" + (i + 1))) as GameObject;
                Transform character = obj.transform;
                character.SetParent(Parent);
                character.localScale = DEFAULTSCALE;
                character.localPosition = new Vector3(i * ADJACENTDISTANCE, 0, -250);
                character.localRotation = Quaternion.identity;
                character.name = "Character " + (i + 1);

                if (character.name.Equals(UIManager.instance.CurrentActiveCharacter.name))
                {
                    setActiveCharacter(character);
                }

                if (selectableCharacterArray == null)
                    selectableCharacterArray = new Transform[TOTALCHARACTERS];

                selectableCharacterArray[i] = character;
            }
        }

        void setActiveCharacter(Transform trnsfrm)
        {
            if (currentActiveCharacter != null)
                Destroy(currentActiveCharacter.gameObject);

            currentActiveCharacter = Instantiate(trnsfrm) as Transform;
            currentActiveCharacter.SetParent(activeCharacterParent);
            currentActiveCharacter.localScale = DEFAULTSCALE;
            currentActiveCharacter.localPosition = new Vector3(0, 0, -250);
            currentActiveCharacter.localRotation = Quaternion.identity;
            currentActiveCharacter.name = trnsfrm.name;

            updateSelectButtonState();
        }

        void updateSelectButtonState()
        {
            selectButton.interactable = !(CharacterNameText.text.Equals(currentActiveCharacter.name));
        }

        void updateHighlightedCharacterInfo()
        {
            CharacterNameText.text = selectableCharacterArray[currentItemIndex].transform.name;
            updateSelectButtonState();
        }

        void Update()
        {
            // Clamp Y, Z axes of the parent [Uncomment this line if the parent moves on Y axis. I had found some bug regarding this issue. Can check my question here. http://answers.unity3d.com/questions/1164375/different-normal-vs-debug-mode-values-in-inspector.html]
            //Parent.localPosition = new Vector3 (Parent.localPosition.x, Y, Z);

            //On drag begin
            if (!move.Equals(Vector2.zero))
            {
                resetCharacterRotation = false;
                isScreenTouched = true;
                if (!isDragging && false)
                {
                    firstTouchPosition = Input.mousePosition;
                    lastTouchPosition = firstTouchPosition;
                    isDragging = true;
                    initLeftRightItemIndex();
                }
                else
                {
                    if (directionChanged)
                        updateLeftRightItemIndex();


                    if (move.Equals(new Vector2(1, 0)))
                    {
                        //Left drag.
                        CurrentDirection = DIRECTION.Left;
                        Parent.localPosition = Vector3.Lerp(Parent.localPosition,
                            new Vector3(Parent.localPosition.x - MOVEBY, Y, Z), Time.deltaTime);
                        //leftSwipe();
                    }
                    else if (move.Equals(new Vector2(-1, 0)))
                    {
                        //Right drag.
                        CurrentDirection = DIRECTION.Right;
                        Parent.localPosition = Vector3.Lerp(Parent.localPosition,
                            new Vector3(Parent.localPosition.x + MOVEBY, Y, Z), Time.deltaTime);
                        //rightSwipe();
                    }
                    //lastTouchPosition = Input.mousePosition;
                }
            }
            else
            {
                //When not dragging.
                if (isScreenTouched)
                {
                    if (isDragging)
                        isDragging = false;

                    /*CurrentDirection = DIRECTION.None;
                    //Calculate distance swiped.
                    if (lastTouchPosition != Vector2.zero)
                    {
                        swipeDistance = lastTouchPosition.x - firstTouchPosition.x;
                        normalizedSwipeDistance = swipeDistance / Screen.width;
                        lastTouchPosition = Vector3.zero;
                    }*/

                    /*if (normalizedSwipeDistance < 0)
                        leftSwipe();
                    else if (normalizedSwipeDistance > 0)
                        rightSwipe();*/

                    //snap to nearest character
                    if (!isCharacterSelectParentSnapped())
                    {
                        int toMove = (int) (Parent.localPosition.x % ADJACENTDISTANCE);

                        //Large swipe. Move to next item.
                        /*if (normalizedSwipeDistance < -SWIPETHRESHOLD || normalizedSwipeDistance > SWIPETHRESHOLD)
                        {
                            toMove += (normalizedSwipeDistance > 0) ? -ADJACENTDISTANCE :
                                (normalizedSwipeDistance < 0) ? ADJACENTDISTANCE : 0;

                            if (Mathf.Abs(toMove) > ADJACENTDISTANCE)
                                toMove %= ADJACENTDISTANCE;
                        }*/

                        Parent.localPosition = Vector3.Lerp(Parent.localPosition,
                            new Vector3(Parent.localPosition.x - toMove, Y, Z), Time.deltaTime * 15);
                        selectableCharacterArray[currentItemIndex].localScale = Vector3.Lerp(
                            selectableCharacterArray[currentItemIndex].localScale, DEFAULTSCALE,
                            Time.deltaTime * ROTATIONSPEED);

                        if ((toMove >= -1 && toMove <= 1) || (Parent.localPosition.x > 0) || (Parent.localPosition.x < -(totalCharacters - 1) * ADJACENTDISTANCE))
                        {
                            //Round off lerp values
                            /*if (toMove >= -1 && toMove <= 1)
                                Parent.localPosition =
                                    new Vector3(
                                        Mathf.RoundToInt(Parent.localPosition.x / ADJACENTDISTANCE) * ADJACENTDISTANCE,
                                        Y, Z);*/

                            //If already on first character snap To First Element
                            /*if (Parent.localPosition.x > 0)
                                Parent.localPosition = new Vector3(0, Y, Z);*/

                            //If already on last character snap To LastElement
                            /*if (Parent.localPosition.x < -(totalCharacters - 1) * ADJACENTDISTANCE)
                                Parent.localPosition = new Vector3(-(totalCharacters - 1) * ADJACENTDISTANCE, Y, Z);

                            isScreenTouched = false;
                            currentItemIndex = -Mathf.RoundToInt(Parent.localPosition.x / ADJACENTDISTANCE);

                            updateHighlightedCharacterInfo();*/
                        }
                    }
                }
                else
                {
                    /*selectableCharacterArray[currentItemIndex].localScale = Vector3.Lerp(
                        selectableCharacterArray[currentItemIndex].localScale, HIGHLIGHTEDSCALE,
                        Time.deltaTime * ROTATIONSPEED);
                    if (!resetCharacterRotation)
                        resetCharacter();*/
                }
            }

            //spin current highlighted character
            /*selectableCharacterArray[currentItemIndex].Rotate(Vector3.up, Time.deltaTime * ROTATIONSPEED);

            //Spin tiny character in top right corner
            if (currentActiveCharacter)
                currentActiveCharacter.Rotate(Vector3.up, Time.deltaTime * ROTATIONSPEED);*/
        }

        void initLeftRightItemIndex()
        {
            leftItemIndex = currentItemIndex - 1;
            rightItemIndex = currentItemIndex + 1;
            middleItemIndex = currentItemIndex;
        }

        void updateLeftRightItemIndex()
        {
            if (CurrentDirection == DIRECTION.Left)
                rightItemIndex = leftItemIndex + 1;
            else if (CurrentDirection == DIRECTION.Right)
                leftItemIndex = rightItemIndex - 1;

            middleItemIndex = -1;
        }

        DIRECTION CurrentDirection
        {
            get { return currentDirection; }
            set
            {
                if (currentDirection != value)
                {
                    oldDirection = currentDirection;
                    currentDirection = value;

                    if (oldDirection != DIRECTION.None)
                    {
                        directionChanged = true;
                    }
                }
                else
                {
                    directionChanged = false;
                }
            }
        }

        void leftSwipe()
        {
            //Scale DOWN leftItem, Scale UP rightItem
            if (leftItemIndex >= 0 && leftItemIndex < totalCharacters)
                selectableCharacterArray[leftItemIndex].localScale = Vector3.Lerp(
                    selectableCharacterArray[leftItemIndex].localScale, DEFAULTSCALE,
                    Time.deltaTime * ONDRAGSCALESPEED);
            if (rightItemIndex >= 0 && rightItemIndex < totalCharacters)
                selectableCharacterArray[rightItemIndex].localScale = Vector3.Lerp(
                    selectableCharacterArray[rightItemIndex].localScale, HIGHLIGHTEDSCALE,
                    Time.deltaTime * ONDRAGSCALESPEED);
            if (middleItemIndex >= 0 && middleItemIndex < totalCharacters)
                selectableCharacterArray[middleItemIndex].localScale = Vector3.Lerp(
                    selectableCharacterArray[middleItemIndex].localScale, DEFAULTSCALE,
                    Time.deltaTime * ONDRAGSCALESPEED);
        }

        void rightSwipe()
        {
            //Scale DOWN rightItem, Scale UP leftItem
            if (leftItemIndex >= 0 && leftItemIndex < totalCharacters)
                selectableCharacterArray[leftItemIndex].localScale = Vector3.Lerp(
                    selectableCharacterArray[leftItemIndex].localScale, HIGHLIGHTEDSCALE,
                    Time.deltaTime * ONDRAGSCALESPEED);
            if (rightItemIndex >= 0 && rightItemIndex < totalCharacters)
                selectableCharacterArray[rightItemIndex].localScale = Vector3.Lerp(
                    selectableCharacterArray[rightItemIndex].localScale, DEFAULTSCALE,
                    Time.deltaTime * ONDRAGSCALESPEED);
            if (middleItemIndex >= 0 && middleItemIndex < totalCharacters)
                selectableCharacterArray[middleItemIndex].localScale = Vector3.Lerp(
                    selectableCharacterArray[middleItemIndex].localScale, DEFAULTSCALE,
                    Time.deltaTime * ONDRAGSCALESPEED);
        }

        void resetCharacter()
        {
            for (int i = 0; i < selectableCharacterArray.Length; i++)
            {
                if (i == currentItemIndex)
                    continue;
                selectableCharacterArray[i].localRotation = Quaternion.identity;
                selectableCharacterArray[i].localScale = DEFAULTSCALE;
            }

            resetCharacterRotation = true;
        }

        bool isCharacterSelectParentSnapped()
        {
            return isScreenTouched && (Mathf.RoundToInt(Parent.localPosition.x % ADJACENTDISTANCE) == 0);
        }

        public void SelectCharacter()
        {
            Transform t = Instantiate(selectableCharacterArray[currentItemIndex]) as Transform;
            t.name = selectableCharacterArray[currentItemIndex].name;
            t.SetParent(UIManager.instance.transform);
            UIManager.instance.CurrentActiveCharacter = t;
            setActiveCharacter(t);
        }

        void OnEnable()
        {
            //Reset to first element
            Time.timeScale = 1f;
            Parent.localPosition = new Vector3(0, Y, Z);
            isScreenTouched = false;
            currentItemIndex = Mathf.Abs(Mathf.RoundToInt(Parent.localPosition.x / ADJACENTDISTANCE));
        }

        Text CharacterNameText
        {
            get { return characterNameText; }
            set { characterNameText = value; }
        }

        public void backButttonClick()
        {
            UIManager.instance.CurrState = UIManager.State.MainMenu;
        }
    }
}
using HarmonyLib;
using System.Xml;
using UnityEngine;

namespace SmolHatchling
{
    public class StoryController : MonoBehaviour
    {
        public static StoryController Instance;
        public AssetBundle _textAssets;
        public bool _storyEnabledNow, _busted;

        public void Awake()
        {
            Instance = this;
            Harmony.CreateAndPatchAll(typeof(StoryController));
        }

        public void Start()
        {
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (_textAssets == null) _textAssets = SmolHatchlingController.Instance.ModHelper.Assets.LoadBundle("Assets/sh_textassets");
                if (SmolHatchlingController.Instance._storyEnabled == false)
                {
                    _storyEnabledNow = false;
                    return;
                }

                _storyEnabledNow = true;
                SetupStory();
                if (LoadManager.s_currentScene == OWScene.SolarSystem) OnSolarSystemLoaded();
            };
        }

        public void OnSolarSystemLoaded()
        {
            // Place page with launch codes
            GameObject gameObject = Instantiate(GameObject.Find("DeepFieldNotes_2"));
            CharacterDialogueTree dialogueTree = gameObject.GetComponentInChildren<CharacterDialogueTree>();
            InteractVolume interactVolume = gameObject.GetComponentInChildren<InteractVolume>();
            GameObject pageModel = gameObject.transform.Find("plaque_paper_1 (1)").gameObject;
            gameObject.name = "SH_LaunchCodesNote";
            gameObject.transform.parent = GameObject.Find("Sector_Village").transform.Find("Sector_Observatory");
            gameObject.transform.localPosition = new Vector3(4.1482f, 0.3305f, 2.3006f);
            gameObject.transform.localRotation = Quaternion.Euler(284.3869f, 346.6762f, 271.0341f);
            interactVolume.transform.localPosition = new Vector3(0.2f, 0.9f, 0.2f);
            interactVolume.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            pageModel.transform.localPosition = new Vector3(0, 0, 0);
            pageModel.transform.localRotation = Quaternion.Euler(60.3462f, 346.2182f, 0);
            Destroy(gameObject.transform.Find("plaque_paper_1 (2)").gameObject);
            Destroy(gameObject.transform.Find("plaque_paper_1 (3)").gameObject);
            foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>())
                renderer.enabled = true;
            dialogueTree._attentionPoint = pageModel.transform;
            dialogueTree._characterName = "SH_LaunchCodesNote";
            dialogueTree._xmlCharacterDialogueAsset = _textAssets.LoadAsset<TextAsset>("SH_LaunchCodesNote");
            ChangeDialogueTree("SH_LaunchCodesNote");
            interactVolume.enabled = true;
            interactVolume.EnableInteraction();

            _busted = false;
        }

        public void SetupStory()
        {
            // Change all dialogue trees
            ChangeAllDialogueTrees();
        }

        public void ChangeAllDialogueTrees()
        {
            var dialogueTrees = FindObjectsOfType<CharacterDialogueTree>();
            for (var i = 0; i < dialogueTrees.Length; ++i)
            {
                CharacterDialogueTree dialogueTree = dialogueTrees[i];
                string assetName = dialogueTree._xmlCharacterDialogueAsset.name;
                if (!_textAssets.Contains(assetName)) continue;
                TextAsset textAsset = _textAssets.LoadAsset<TextAsset>(assetName);
                dialogueTree.SetTextXml(textAsset);
                AddTranslations(textAsset.ToString());
                dialogueTree.OnDialogueConditionsReset();
            }
        }

        public void ChangeDialogueTree(string dialogueName)
        {
            var dialogueTrees = FindObjectsOfType<CharacterDialogueTree>();
            for (var i = 0; i < dialogueTrees.Length; ++i)
            {
                CharacterDialogueTree dialogueTree = dialogueTrees[i];
                string assetName = dialogueTree._xmlCharacterDialogueAsset.name;
                if (dialogueTree.name != dialogueName || !_textAssets.Contains(assetName)) continue;
                TextAsset textAsset = _textAssets.LoadAsset<TextAsset>(assetName);
                dialogueTree.SetTextXml(textAsset);
                AddTranslations(textAsset.ToString());
                dialogueTree.OnDialogueConditionsReset();
            }
        }

        public void AddTranslations(string textAsset)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(textAsset);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("DialogueTree");
            XmlNodeList xmlNodeList = xmlNode.SelectNodes("DialogueNode");
            string NameField = xmlNode.SelectSingleNode("NameField").InnerText;
            var translationTable = TextTranslation.Get().m_table.theTable;
            translationTable[NameField] = NameField;
            foreach (object obj in xmlNodeList)
            {
                XmlNode xmlNode2 = (XmlNode)obj;
                var name = xmlNode2.SelectSingleNode("Name").InnerText;

                XmlNodeList xmlText = xmlNode2.SelectNodes("Dialogue/Page");
                foreach (object Page in xmlText)
                {
                    XmlNode pageData = (XmlNode)Page;
                    translationTable[name + pageData.InnerText] = pageData.InnerText;
                }
                xmlText = xmlNode2.SelectNodes("DialogueOptionsList/DialogueOption/Text");
                foreach (object Page in xmlText)
                {
                    XmlNode pageData = (XmlNode)Page;
                    translationTable[NameField + name + pageData.InnerText] = pageData.InnerText;

                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChertDialogueSwapper), nameof(ChertDialogueSwapper.SelectMood))]
        public static void ChertDialogueSwapped()
        {
            StoryController.Instance.ChangeDialogueTree("Chert");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.EndConversation))]
        public static void EndConversation()
        {
            if (DialogueConditionManager.s_instance.GetConditionState("Busted") && !StoryController.Instance._busted)
            {
                StoryController.Instance._busted = true;
                Locator.GetShipBody().GetComponentInChildren<ShipCockpitController>().LockUpControls(Mathf.Infinity);
                NotificationManager.s_instance.PostNotification(new NotificationData(NotificationTarget.Player, "SHIP HAS BEEN DISABLED BY GROUND CONTROL", 5f), false);
                NotificationManager.s_instance.PostNotification(new NotificationData(NotificationTarget.Ship, "SHIP DISABLED"), true);
            }
        }
    }
}
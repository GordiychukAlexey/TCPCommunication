using UnityEngine;
using UnityEngine.UI;

namespace UI {
	public class ControlPanel : MonoBehaviour {
		public InputField PortInputField;
		public Button RunServerButton;
		public Button StopServerButton;

		public InputField HostInputField;
		public Button RunClientButton;
		public Button StopClientButton;

		public Text LogText;
		[SerializeField] private Button ClearLogButton;

		private void Awake(){
			ClearLogButton.onClick.AddListener(() => LogText.text = "");
		}
	}
}
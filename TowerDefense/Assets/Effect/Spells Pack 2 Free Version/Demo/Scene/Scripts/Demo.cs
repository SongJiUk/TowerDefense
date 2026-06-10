using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZakhanSpellsPack2
{
    public class Demo : MonoBehaviour
    {
		[Serializable]
		class VFXData
		{
            public string Name;
			public GameObject VFX;
			public List<GameObject> Dummies = new List<GameObject>();
		}
		[SerializeField] private List<VFXData> VFX = new List<VFXData>();
		[SerializeField] private Transform DummiesGroup;
		[SerializeField] private Demo_Cinemachine_Impulses ImpulsesController;
		[SerializeField] private int CurrentSelection = 0;

		public InputSystem_Actions InputAction;

		[Header("UI Settings")]
		[SerializeField] private Demo_UI UI;
		private void Awake()
		{
			InputAction = new InputSystem_Actions();

			for (int i = 0; i < DummiesGroup.childCount; i++)
			{
				GameObject Dummy = DummiesGroup.transform.GetChild(i).gameObject;
				Dummy.SetActive(false);
			}

		}

		void Start()
		{
			CurrentSelection = 0;
			VFX[CurrentSelection].VFX.gameObject.SetActive(true);
			Dummies(true);

			//UI
			UpdateUI();

		}

		private void OnEnable()
		{
			InputAction.Enable();
			InputAction.Player.Next.performed += Next_Performed;
			InputAction.Player.Previous.performed += Back_Performed;
			InputAction.UI.HideUI.performed += HideUI_performed;
		}

		private void OnDisable()
		{
			InputAction.Disable();
			InputAction.Player.Next.performed -= Next_Performed;
			InputAction.Player.Previous.performed -= Back_Performed;
			InputAction.UI.HideUI.performed -= HideUI_performed;
		}

		private void HideUI_performed(InputAction.CallbackContext context)
		{
			UI.EnableCanvas();
		}

		private void Next_Performed(InputAction.CallbackContext context)
		{
			Next();
		}

		private void Back_Performed(InputAction.CallbackContext context)
		{
			Back();	
		}
		public void Next()
		{
			if (CurrentSelection >= 0 && CurrentSelection != VFX.Count - 1)
			{
				Dummies(false);
				VFX[CurrentSelection].VFX.gameObject.SetActive(false);
				ImpulsesController.StopImpulse(); // Stopping Impulses to avoid overlap with other VFXs
				CurrentSelection++;
				VFX[CurrentSelection].VFX.gameObject.SetActive(true);
				Dummies(true);

				//UI
				UpdateUI();
			}
		}
		public void Back()
		{
			if (CurrentSelection > 0)
			{
				Dummies(false);
				VFX[CurrentSelection].VFX.gameObject.SetActive(false);
				ImpulsesController.StopImpulse(); // Stopping Impulses to avoid overlap with other VFXs
				CurrentSelection--;
				VFX[CurrentSelection].VFX.gameObject.SetActive(true);
				Dummies(true);

				//UI
				UpdateUI();
			}
		}
		private void UpdateUI()
		{
			//UI
			UI.ChangeName(VFX[CurrentSelection].Name);

			switch (CurrentSelection)
			{
				case 0:
					UI.EnableBackButton(false);
					UI.EnableNextButton(true);
					break;
				case 1:
					UI.EnableBackButton(true);
					UI.EnableNextButton(false);
					break;
				case int s when (s >= 0 && s == VFX.Count - 1):
					UI.EnableNextButton(false);
					break;
				case int s when (s >= 0 && s != VFX.Count - 1):
					UI.EnableNextButton(true);
					break;
			}
		}

		private void Dummies(bool State)
		{
			foreach(GameObject Dummy in VFX[CurrentSelection].Dummies)
			{
				Dummy.gameObject.SetActive(State);
			}
		}

    }
}

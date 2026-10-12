using System.Collections;
using System.Collections.Generic;
using Game.Domain;
using Game.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Game.Tests.PlayMode
{
    public class MachineViewPlayModeTests
    {
        private readonly List<Object> _createdObjects = new();

        [UnityTest]
        public IEnumerator MachineView_UpdatesUIOnBalanceChange()
        {
            var machineConfig = new FakeMachineConfig(
                "machine_1",
                100d,
                1d,
                10d,
                1.5d,
                false);
            var config = new FakeConfig(machineConfig);
            var economy = new EconomyService(config, config.Machines);
            var machineViewObject = new GameObject("MachineView");
            var buttonObject = new GameObject("ActionButton");
            var labelObject = new GameObject("ButtonLabel");

            _createdObjects.Add(machineViewObject);
            _createdObjects.Add(buttonObject);
            _createdObjects.Add(labelObject);

            MachineView machineView =
                machineViewObject.AddComponent<MachineView>();
            Button actionButton = buttonObject.AddComponent<Button>();
            TMP_Text buttonLabel =
                labelObject.AddComponent<TextMeshProUGUI>();

            SetPrivateField(machineView, "_actionButton", actionButton);
            SetPrivateField(machineView, "_buttonLabel", buttonLabel);

            machineView.Bind(
                machineConfig,
                economy.Machines[0],
                economy);

            Assert.That(buttonLabel.text, Is.EqualTo("Unlock (100)"));

            economy.TryAddCoins(50d);
            yield return null;

            Assert.That(buttonLabel.text, Is.EqualTo("Unlock (100)"));
        }

        private static void SetPrivateField(
            object target,
            string fieldName,
            object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic);

            field.SetValue(target, value);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object createdObject in _createdObjects)
            {
                if (createdObject != null)
                {
                    Object.Destroy(createdObject);
                }
            }

            _createdObjects.Clear();
        }
    }
}

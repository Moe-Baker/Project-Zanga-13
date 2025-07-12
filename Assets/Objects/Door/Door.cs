using System;

using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    bool IsOpen;

    [SerializeField]
    PaneData[] Panes;
    [Serializable]
    public struct PaneData
    {
        [SerializeField]
        internal Transform Transform;

        [SerializeField]
        float Speed;

        [SerializeField]
        Vector3 Open;

        [SerializeField]
        Vector3 Closed;

        public Vector3 Evaluate(bool isOpen) => isOpen ? Open : Closed;

        public void Animate(bool isOpen)
        {
            var position = Evaluate(isOpen);

            Transform.localPosition = Vector3.MoveTowards(Transform.localPosition, position, Speed * Time.deltaTime);
        }

        public void Validate(bool isOpen)
        {
            Transform.localPosition = Evaluate(isOpen);
        }
    }


    void OnValidate()
    {
        if (Application.isPlaying)
            return;

        foreach (var pane in Panes)
            pane.Validate(IsOpen);
    }

    void Update()
    {
        foreach (var pane in Panes)
            pane.Animate(IsOpen);
    }
}
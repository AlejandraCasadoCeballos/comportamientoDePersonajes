using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionNode : Node
{
    public enum Reevaluation { onlyOnEnd, atFixedRate};

    Action onBegin;
    Action onUpdate;
    Action onEnd;

    public float reevaluationRate;
    public Reevaluation reevaluationMode;

    public ActionNode(float reevaluationRate = 1f, Reevaluation reevaluationMode = Reevaluation.onlyOnEnd)
    {
        this.reevaluationRate = reevaluationRate;
        this.reevaluationMode = reevaluationMode;
    }

    public void Begin()
    {
        onBegin?.Invoke();
    }

    public void Update()
    {
        onUpdate?.Invoke();
    }

    public void End()
    {
        onEnd?.Invoke();
    }

    public ActionNode SetOnBegin(Action action)
    {
        onBegin = action;
        return this;
    }

    public ActionNode SetOnUpdate(Action action)
    {
        onUpdate = action;
        return this;
    }

    public ActionNode SetOnEnd(Action action)
    {
        onEnd = action;
        return this;
    }
}

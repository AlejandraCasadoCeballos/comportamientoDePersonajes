﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionNode : Node
{
    public enum Reevaluation { onlyOnEnd, atFixedRate};

    public event Action onActionEnded;
    public event Action onActionBegin;

    Action onBegin;
    Action onUpdate;
    Action onEnd;

    public float reevaluationRate;
    public Reevaluation reevaluationMode;

    private bool hasEnded;

    public ActionNode(float reevaluationRate = 1f, Reevaluation reevaluationMode = Reevaluation.onlyOnEnd)
    {
        this.reevaluationRate = reevaluationRate;
        this.reevaluationMode = reevaluationMode;
        hasEnded = false;
    }

    public void Begin()
    {
        onBegin?.Invoke();
        onActionBegin?.Invoke();
        hasEnded = false;
    }

    public void Update()
    {
        if(!hasEnded)
            onUpdate?.Invoke();
    }

    public void End()
    {
        onEnd?.Invoke();
        onActionEnded?.Invoke();
        hasEnded = true;
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
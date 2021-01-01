using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DronADCACBehaviour : DronBehaviour
{
    [SerializeField] float movementSpeed;
    [SerializeField] float attackRange;

    UtilitySystem utilitySystem;
    Evaluator evaluator;
    SphereCollider attackRangeTrigger;


    private void Start()
    {
        attackRangeTrigger = GetComponent<SphereCollider>();


        CreateUtilitySystem();
    }

    private void CreateUtilitySystem()
    {
        PerceptionNode enemyDistance = new PerceptionNode(() => CheckDistance());
        PerceptionNode waitingRecruiter = new PerceptionNode(() => CheckRecruiterWaiting());
        PerceptionNode conquestSignal = new PerceptionNode(() => CheckConquestSignal());
        PerceptionNode allyBaseIsBeingConquered = new PerceptionNode(() => CheckAllyBaseIsBeingConquered());

        ActionNode attackEnemy = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(
            () => AttackEnemy());

        ActionNode goWaitingPoint = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(
            () => GoWaitingPoint());

        ActionNode conquerEnemyBase = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(
            () => ConquerEnemyBase());

        ActionNode defendAllyBase = new ActionNode(1f, ActionNode.Reevaluation.atFixedRate).SetOnBegin(
            () => DefendAllyBase());

        UtilityFunction u1 = new UtilityFunction(attackEnemy, enemyDistance).SetOnEvaluateUtility(
            (float per1) => per1);

        UtilityFunction u2 = new UtilityFunction(goWaitingPoint, waitingRecruiter).SetOnEvaluateUtility(
            (float per2) => per2);

        UtilityFunction u3 = new UtilityFunction(conquerEnemyBase, conquestSignal).SetOnEvaluateUtility(
            (float per3) => per3);

        UtilityFunction u4 = new UtilityFunction(defendAllyBase, allyBaseIsBeingConquered).SetOnEvaluateUtility(
            (float per4) => per4);


        utilitySystem = new UtilitySystem();
        utilitySystem.SetUtilityFunctions(new HashSet<UtilityFunction>() { u1, u2, u3, u4 });
        evaluator = GetComponent<Evaluator>();
        evaluator.SetBehaviour(utilitySystem);
    }

    private float CheckDistance()
    {
        float distance=0f;

        //Calcular la distancia a cada enemigo dentro de un rango=?

        return distance;
    }
    private float CheckRecruiterWaiting()
    {
        bool isWaiting = true;

        //Mirar si hay algún reclutador esperando

        return isWaiting ? 1.0f : 0.0f;
    }
    private float CheckConquestSignal()
    {
        bool conquer = true;

        //Mirar si el reclutador ha dado orden de conquista

        return conquer ? 1.0f : 0.0f;
    }
    private float CheckAllyBaseIsBeingConquered()
    {
        bool conquering = true;

        //Mirar si una base cercana está siendo conquistada

        return conquering ? 1.0f : 0.0f;
    }
    private void AttackEnemy()
    {
        //FSM de ataque
    }
    private void GoWaitingPoint()
    {
        //FSM de ir al punto de espera del reclutador
    }
    private void ConquerEnemyBase()
    {
        //FSM conquistar base
    }
    private void DefendAllyBase()
    {
        //FSM defender base 
    }
}

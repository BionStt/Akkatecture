﻿using System;
using System.Linq.Expressions;
using Akka.Actor;
using Akka.Event;
using Akkatecture.Aggregates;
using Akkatecture.Extensions;

namespace Akkatecture.Sagas
{
    public abstract class SagaManager<TSaga, TSagaId, TSagaLocator> : ReceiveActor
        where TSagaId : ISagaId
        where TSagaLocator : ISagaLocator<TSagaId>
        where TSaga : Saga<TSagaId, SagaState<TSaga,TSagaId>>
    {
        protected ILoggingAdapter Logger { get; set; }
        private readonly Expression<Func<TSaga>> SagaFactory;
        private TSagaLocator SagaLocator { get; }

        protected SagaManager(Expression<Func<TSaga>> sagaFactory)
        {
            Logger = Context.GetLogger();

            Context.System.EventStream.Subscribe(Self, typeof(DeadLetter));

            SagaLocator = (TSagaLocator)Activator.CreateInstance(typeof(TSagaLocator));

            SagaFactory = sagaFactory;
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 3,
                withinTimeMilliseconds: 3000,
                localOnlyDecider: x =>
                {

                    Logger.Error($"[{GetType().PrettyPrint()}] Exception={x.ToString()} to be decided.");
                    return Directive.Restart;
                });
        }

        protected IActorRef FindOrSpawn(TSagaId sagaId)
        {
            var saga = Context.Child(sagaId);
            if (Equals(saga, ActorRefs.Nobody))
            {
                return Spawn(sagaId);
            }
            return saga;
        }

        private IActorRef Spawn(TSagaId sagaId)
        {
            var saga = Context.ActorOf(Props.Create(SagaFactory), sagaId.Value);
            Context.Watch(saga);
            return saga;
        }
        
    }

    public class FooSagaManager : SagaManager<FooSaga, FooSagaId, FooSagaLocator>
    {
        public FooSagaManager(Expression<Func<FooSaga>> sagaFactory)
            : base(sagaFactory)
        {
            
        }
    }

    public class FooSaga : Saga<FooSagaId, SagaState<FooSaga,FooSagaId>>
    {
        public FooSaga(int i, string q, long j)
        {
            
        }
    }

    public class FooSagaLocator : ISagaLocator<FooSagaId>
    {
        public FooSagaId LocateSaga(IDomainEvent domainEvent)
        {
            var sagaId = domainEvent.GetIdentity();
            return new FooSagaId($"foosaga-{sagaId}");
        }
    }

    public class FooSagaId : SagaId<FooSagaId>
    {
        public FooSagaId(string value)
            : base(value)
        {
            
        }
    }

    public class FooSagaState : SagaState<FooSaga, FooSagaId>
    {
        
    }
}

﻿using Akkatecture.Sagas;

namespace Akkatecture.TestHelpers.Aggregates.Sagas
{
    public class TestSaga : Saga<TestSagaId, SagaState<TestSaga,TestSagaId>>
    {
    }
}

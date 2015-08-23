﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Moq.Modules;
using Prius.Contracts.Interfaces;

namespace Prius.Mocks
{
    public class MockRepositoryFactory: MockImplementationProvider<IRepositoryFactory>
    {
        protected override void SetupMock(IMockProducer mockProducer, Mock<IRepositoryFactory> mock)
        {
            mock.Setup(rf => rf.Create(It.IsAny<string>()))
                .Returns(mockProducer.SetupMock<IRepository>());
        }
    }
}

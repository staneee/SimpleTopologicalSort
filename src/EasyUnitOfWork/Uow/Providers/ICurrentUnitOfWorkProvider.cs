﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EasyUnitOfWork.Uow.Providers
{
    /// <summary>
    /// 工作单元提供者
    /// </summary>
    public interface ICurrentUnitOfWorkProvider
    {
        /// <summary>
        /// 当前的工作单元
        /// </summary>
        IUnitOfWork Current { get; set; }
    }

}
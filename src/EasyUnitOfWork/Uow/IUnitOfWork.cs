﻿using EasyUnitOfWork.Uow.Handles;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyUnitOfWork.Uow
{
    public interface IUnitOfWork : IActiveUnitOfWork, IUnitOfWorkCompleteHandle
    {
        /// <summary>
        /// 工作单元id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Reference to the outer UOW if exists.
        /// </summary>
        IUnitOfWork Outer { get; set; }

        /// <summary>
        /// 根据工作单元选项创建
        /// </summary>
        /// <param name="options">Unit of work options</param>
        void Begin(UnitOfWorkOptions options);
    }
}

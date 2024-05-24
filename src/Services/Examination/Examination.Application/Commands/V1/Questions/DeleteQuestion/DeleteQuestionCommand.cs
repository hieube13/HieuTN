﻿using Examination.Shared.SeedWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examination.Application.Commands.V1.Questions.DeleteQuestion
{
    public class DeleteQuestionCommand : IRequest<ApiResult<bool>>
    {
        public DeleteQuestionCommand(string id)
        {
            Id = id;
        }
        public string Id { get; set; }
    }
}

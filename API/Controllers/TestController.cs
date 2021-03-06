﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCore.Entities.EssayExerciseAggregate;
using ApplicationCore.Entities.MultipleChoicesExerciseAggregate;
using ApplicationCore.Entities.TestAggregate;
using ApplicationCore.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Test;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : BaseApiController
    {
        #region fields
        private readonly IUnitOfWork _unitOfWork;
        private readonly IService _service;
        private readonly IMapper _mapper;
        #endregion

        #region ctor
        public TestController(IUnitOfWork unitOfWork,
            IService service,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _service = service;
            _mapper = mapper;
        }
        #endregion
        #region check if existing, add, update, delete
        //get  Test in test by test id


        //get  Test by id
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTestById([FromRoute] int id)
        {
            var test = await _service.TestService.GetByIdAsync(id);
            if (test == null)
            {
                return ErrorResult($"Can not found test with Id: {id}");
            }
            var testRes = new TestModel
            {
                Name = test.Name,
                Id = test.Id,
                Type = (Models.Test.TestType)test.Type
            };
            return SuccessResult(testRes, "Get test successfully.");
        }

        //add new  Test
        /// <summary>
        /// <response code=200>Request completed Successfully</response>
        /// <response code=400>Bad Request</response>
        /// <response code=401>Don't have permission</response>
        /// <response code=500>Internal server error</response>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTest([FromBody] TestModel model)
        {
            var newTest = new Test();
            try
            {
                if (!ModelState.IsValid)
                {
                    ValidModel();
                }

                newTest = new Test()
                {
                    Type = (ApplicationCore.Entities.TestAggregate.TestType)model.Type,
                    Name = model.Name
                };
                await _service.TestService.AddAsync(newTest);
            }
            catch (Exception e)
            {
                return ErrorResult(e.Message);
            }
            return SuccessResult(newTest, "Created Test successfully.");
        }




        //Add Test Model -> contains 28 multiple choices & 8 essay
        [AllowAnonymous]
        [HttpPost("create-contains")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTestContainsExercises([FromBody] TestContainerModel model)
        {
            var newTest = new Test();
            try
            {
                if (!ModelState.IsValid)
                {
                    ValidModel();
                }

                //Add Test
                newTest = new Test()
                {
                    Type = (ApplicationCore.Entities.TestAggregate.TestType)model.Type,
                    Name = model.Name
                };
                await _service.TestService.AddAsync(newTest);

                //add multiple exercise
                foreach (var multipleExercise in model.MultipleChoicesExerciseModels)
                {
                    var newMultipleExercise = new MultipleChoicesExercise()
                    {
                        TestId = newTest.Id,
                        RightResult = multipleExercise.RightResult,
                        FalseResult1 = multipleExercise.FalseResult1,
                        FalseResult2 = multipleExercise.FalseResult2,
                        FalseResult3 = multipleExercise.FalseResult3,
                        Title = multipleExercise.Title
                    };
                    await _service.MultipleChoicesExerciseService.AddAsync(newMultipleExercise);
                }

                //add essay exercise
                foreach (var essayExercise in model.EssayExerciseModels)
                {
                    var newEssayExerecise = new EssayExercise()
                    {
                        TestId = newTest.Id,
                        Title = essayExercise.Title,
                        Result = essayExercise.Result
                    };
                    await _service.EssayExerciseService.AddAsync(newEssayExerecise);
                }
            }
            catch (Exception e)
            {
                return ErrorResult(e.Message);
            }
            return SuccessResult(newTest, "Created Test successfully.");
        }




        #endregion
    }
}

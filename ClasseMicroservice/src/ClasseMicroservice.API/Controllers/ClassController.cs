using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClasseMicroservice.Domain.Entities;
using ClasseMicroservice.Domain.Interfaces;

namespace ClasseMicroservice.API.Controllers
{
    [ApiController]
    [Route("api/v1/classes")]
    public class ClassController : ControllerBase
    {
        private readonly ClasseMicroservice.Application.Queries.IQueryHandler<ClasseMicroservice.Application.Queries.GetClassesQuery, System.Collections.Generic.List<ClasseMicroservice.Domain.Entities.Class>> _getClassesHandler;
        private readonly ClasseMicroservice.Application.Queries.IQueryHandler<ClasseMicroservice.Application.Queries.GetClassByIdQuery, ClasseMicroservice.Domain.Entities.Class> _getByIdHandler;
        private readonly ClasseMicroservice.Application.Commands.ICommandHandler<ClasseMicroservice.Application.Commands.CreateClassCommand> _createHandler;
        private readonly ClasseMicroservice.Application.Commands.ICommandHandler<ClasseMicroservice.Application.Commands.UpdateClassCommand> _updateHandler;
        private readonly ClasseMicroservice.Application.Commands.ICommandHandler<ClasseMicroservice.Application.Commands.DeleteClassCommand> _deleteHandler;

        public ClassController(
            ClasseMicroservice.Application.Queries.IQueryHandler<ClasseMicroservice.Application.Queries.GetClassesQuery, System.Collections.Generic.List<ClasseMicroservice.Domain.Entities.Class>> getClassesHandler,
            ClasseMicroservice.Application.Queries.IQueryHandler<ClasseMicroservice.Application.Queries.GetClassByIdQuery, ClasseMicroservice.Domain.Entities.Class> getByIdHandler,
            ClasseMicroservice.Application.Commands.ICommandHandler<ClasseMicroservice.Application.Commands.CreateClassCommand> createHandler,
            ClasseMicroservice.Application.Commands.ICommandHandler<ClasseMicroservice.Application.Commands.UpdateClassCommand> updateHandler,
            ClasseMicroservice.Application.Commands.ICommandHandler<ClasseMicroservice.Application.Commands.DeleteClassCommand> deleteHandler)
        {
            _getClassesHandler = getClassesHandler;
            _getByIdHandler = getByIdHandler;
            _createHandler = createHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] ClasseMicroservice.API.DTOs.CreateClassDto dto)
        {
            if (dto == null)
                return BadRequest("Class payload is required");

            // Map DTO -> Domain entity. Backend (handler) vai gerar os ids quando necessário.
            var domainClass = new ClasseMicroservice.Domain.Entities.Class
            {
                ClassNumber = dto.ClassNumber,
                Year = dto.Year,
                Semester = dto.Semester,
                Schedule = dto.Schedule,
                Course = dto.Course != null ? new ClasseMicroservice.Domain.Entities.Course { Id = dto.Course.Id } : null,
                Exams = dto.Exam != null ? new System.Collections.Generic.List<ClasseMicroservice.Domain.Entities.Exam> { new ClasseMicroservice.Domain.Entities.Exam { Id = dto.Exam.Id, Name = dto.Exam.Name, Date = dto.Exam.Date, Weight = dto.Exam.Weight } } : new System.Collections.Generic.List<ClasseMicroservice.Domain.Entities.Exam>(),
                Students = dto.Student != null ? new System.Collections.Generic.List<ClasseMicroservice.Domain.Entities.Student> { new ClasseMicroservice.Domain.Entities.Student { Id = dto.Student.Id } } : new System.Collections.Generic.List<ClasseMicroservice.Domain.Entities.Student>(),
                Professors = dto.Professor != null ? new System.Collections.Generic.List<ClasseMicroservice.Domain.Entities.Professor> { new ClasseMicroservice.Domain.Entities.Professor { Id = dto.Professor.Id } } : new System.Collections.Generic.List<ClasseMicroservice.Domain.Entities.Professor>()
            };

            var command = new ClasseMicroservice.Application.Commands.CreateClassCommand(domainClass);
            await _createHandler.HandleAsync(command);

            return CreatedAtAction(nameof(GetClassById), new { id = domainClass.Id }, domainClass);
        }

        [HttpGet]
        public async Task<IActionResult> GetClasses([FromQuery] int? year, [FromQuery] int? semester, [FromQuery] string course_id, [FromQuery] int? page, [FromQuery] int? size)
        {
            var query = new ClasseMicroservice.Application.Queries.GetClassesQuery(year, semester, course_id, page ?? 1, size ?? 10);
            var classes = await _getClassesHandler.HandleAsync(query);
            return Ok(classes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassById(string id)
        {
            var query = new ClasseMicroservice.Application.Queries.GetClassByIdQuery(id);
            var classObj = await _getByIdHandler.HandleAsync(query);
            if (classObj == null)
                return NotFound();
            return Ok(classObj);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClass(string id, [FromBody] ClasseMicroservice.Domain.Entities.Class classDto)
        {
            if (classDto == null)
                return BadRequest("Class payload is required");

            var command = new ClasseMicroservice.Application.Commands.UpdateClassCommand(id, classDto);
            await _updateHandler.HandleAsync(command);
            return Ok(classDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClass(string id)
        {
            var command = new ClasseMicroservice.Application.Commands.DeleteClassCommand(id);
            await _deleteHandler.HandleAsync(command);
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchClass(string id, [FromBody] Dictionary<string, object> updates)
        {
            // TODO: Implement partial update logic
            return Ok();
        }

        // Exams endpoints
        [HttpPost("{id}/exams")]
        public async Task<IActionResult> AddExam(string id, [FromBody] Exam examDto)
        {
            // TODO: Add exam to class
            return Created("", examDto);
        }

        [HttpGet("{id}/exams")]
        public async Task<IActionResult> GetExams(string id)
        {
            // TODO: Get exams for class
            return Ok(new List<Exam>());
        }

        [HttpPut("{id}/exams/{examId}")]
        public async Task<IActionResult> UpdateExam(string id, string examId, [FromBody] Exam examDto)
        {
            // TODO: Update exam
            return Ok(examDto);
        }

        [HttpDelete("{id}/exams/{examId}")]
        public async Task<IActionResult> DeleteExam(string id, string examId)
        {
            // TODO: Delete exam
            return NoContent();
        }

        [HttpPatch("{id}/exams/{examId}")]
        public async Task<IActionResult> PatchExam(string id, string examId, [FromBody] Dictionary<string, object> updates)
        {
            // TODO: Implement partial update for exam
            return Ok();
        }
    }
}

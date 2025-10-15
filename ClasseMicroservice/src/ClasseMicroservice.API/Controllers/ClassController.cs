using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class ClassController : ControllerBase
    {
        private readonly IClassRepository _classRepository;

        public ClassController(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] Class classDto)
        {
            // TODO: Validate, check permissions, handle errors
            await _classRepository.CreateAsync(classDto);
            return CreatedAtAction(nameof(GetClassById), new { id = classDto.Id }, classDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetClasses([FromQuery] int? year, [FromQuery] int? semester, [FromQuery] string course_id, [FromQuery] int? page, [FromQuery] int? size)
        {
            // TODO: Filtering, pagination
            var classes = await _classRepository.GetAllAsync();
            return Ok(classes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassById(string id)
        {
            var classObj = await _classRepository.GetByIdAsync(id);
            if (classObj == null)
                return NotFound();
            return Ok(classObj);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClass(string id, [FromBody] Class classDto)
        {
            // TODO: Validate, check permissions, handle errors
            await _classRepository.UpdateAsync(id, classDto);
            return Ok(classDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClass(string id)
        {
            await _classRepository.DeleteAsync(id);
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Swashbuckle.AspNetCore.Annotations;

namespace ClasseMicroservice.API.Controllers
{
    [ApiController]
    [Route("api/v1/classes")]
    [SwaggerTag("Classes")]
    [Produces("application/json")]
    public class ClassesController : ControllerBase
    {
        private readonly ClasseMicroservice.Application.Queries.IQueryHandler<ClasseMicroservice.Application.Queries.GetClassesQuery, System.Collections.Generic.List<ClasseMicroservice.Domain.Entities.Class>> _getClassesHandler;
        private readonly ClasseMicroservice.Application.Queries.IQueryHandler<ClasseMicroservice.Application.Queries.GetClassByIdQuery, ClasseMicroservice.Domain.Entities.Class> _getByIdHandler;
        private readonly ClasseMicroservice.Application.Commands.ICommandHandler<ClasseMicroservice.Application.Commands.CreateClassCommand> _createHandler;
        private readonly ClasseMicroservice.Application.Commands.ICommandHandler<ClasseMicroservice.Application.Commands.UpdateClassCommand> _updateHandler;
        private readonly ClasseMicroservice.Application.Commands.ICommandHandler<ClasseMicroservice.Application.Commands.DeleteClassCommand> _deleteHandler;

        public ClassesController(
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

        // --- Classes endpoints ---

        [HttpPost]
        [SwaggerOperation(
            Summary = "Criar uma nova classe",
            Description = "Cria uma classe com curso (apenas Id), e opcionalmente provas, alunos e professores.",
            OperationId = "CreateClass",
            Tags = new[] { "Classes" }
        )]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateClass([FromBody] ClasseMicroservice.API.DTOs.CreateClassDto dto)
        {
            if (dto == null)
                return BadRequest("Class payload is required");

            var domainClass = new ClasseMicroservice.Domain.Entities.Class
            {
                ClassNumber = dto.ClassNumber,
                Year = dto.Year,
                Semester = dto.Semester,
                Schedule = dto.Schedule,
                Course = dto.Course != null ? new ClasseMicroservice.Domain.Entities.Course { Id = dto.Course.Id } : null,
                Exams = dto.Exam != null ? new List<ClasseMicroservice.Domain.Entities.Exam> { new ClasseMicroservice.Domain.Entities.Exam { Id = dto.Exam.Id, Name = dto.Exam.Name, Date = dto.Exam.Date, Weight = dto.Exam.Weight } } : new List<ClasseMicroservice.Domain.Entities.Exam>(),
                Students = dto.Student != null ? new List<ClasseMicroservice.Domain.Entities.Student> { new ClasseMicroservice.Domain.Entities.Student { Id = dto.Student.Id } } : new List<ClasseMicroservice.Domain.Entities.Student>(),
                Professors = dto.Professor != null ? new List<ClasseMicroservice.Domain.Entities.Professor> { new ClasseMicroservice.Domain.Entities.Professor { Id = dto.Professor.Id } } : new List<ClasseMicroservice.Domain.Entities.Professor>()
            };

            var command = new ClasseMicroservice.Application.Commands.CreateClassCommand(domainClass);
            await _createHandler.HandleAsync(command);

            return CreatedAtRoute("GetClassById", new { id = domainClass.Id }, domainClass);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Listar classes",
            Description = "Lista classes com filtros opcionais por ano, semestre e course_id, com paginação.",
            OperationId = "GetClasses",
            Tags = new[] { "Classes" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClasses(
            [FromQuery] int? year = null,
            [FromQuery] int? semester = null,
            [FromQuery(Name = "course_id")] string? courseId = null,
            [FromQuery] int? page = null,
            [FromQuery] int? size = null)
        {
            var query = new ClasseMicroservice.Application.Queries.GetClassesQuery(
                year,
                semester,
                courseId,               // pode ser null
                page ?? 1,
                size ?? 10
            );

            var classes = await _getClassesHandler.HandleAsync(query);
            return Ok(classes);
        }

        [HttpGet("{id}", Name = "GetClassById")]
        [SwaggerOperation(
            Summary = "Obter classe por ID",
            Description = "Retorna a classe correspondente ao ID informado.",
            OperationId = "GetClassById",
            Tags = new[] { "Classes" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClassById(string id)
        {
            var query = new ClasseMicroservice.Application.Queries.GetClassByIdQuery(id);
            var classObj = await _getByIdHandler.HandleAsync(query);
            if (classObj == null)
                return NotFound();
            return Ok(classObj);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Atualizar classe",
            Description = "Atualiza integralmente os dados de uma classe existente.",
            OperationId = "UpdateClass",
            Tags = new[] { "Classes" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateClass(string id, [FromBody] ClasseMicroservice.Domain.Entities.Class classDto)
        {
            if (classDto == null)
                return BadRequest("Class payload is required");

            classDto.Id = id;

            var command = new ClasseMicroservice.Application.Commands.UpdateClassCommand(id, classDto);
            await _updateHandler.HandleAsync(command);
            return Ok(classDto);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Excluir classe",
            Description = "Remove uma classe pelo ID.",
            OperationId = "DeleteClass",
            Tags = new[] { "Classes" }
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteClass(string id)
        {
            var command = new ClasseMicroservice.Application.Commands.DeleteClassCommand(id);
            await _deleteHandler.HandleAsync(command);
            return NoContent();
        }

        [HttpPatch("{id}")]
        [SwaggerOperation(
            Summary = "Atualização parcial da classe",
            Description = "Aplica alterações parciais via dicionário de propriedades.",
            OperationId = "PatchClass",
            Tags = new[] { "Classes" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PatchClass(string id, [FromBody] Dictionary<string, object> updates)
        {
            if (updates == null || updates.Count == 0)
                return BadRequest("Updates payload is required");

            var query = new ClasseMicroservice.Application.Queries.GetClassByIdQuery(id);
            var classObj = await _getByIdHandler.HandleAsync(query);
            if (classObj == null)
                return NotFound();

            ApplyDictionaryToObject(classObj, updates);

            var command = new ClasseMicroservice.Application.Commands.UpdateClassCommand(id, classObj);
            await _updateHandler.HandleAsync(command);
            return Ok(classObj);
        }

        // --- Exams endpoints (sub-recurso de classes) ---

        [HttpPost("{id}/exams")]
        [SwaggerOperation(
            Summary = "Adicionar prova à classe",
            Description = "Inclui uma nova prova na classe informada.",
            OperationId = "AddExam",
            Tags = new[] { "Exams" }
        )]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddExam(string id, [FromBody] ClasseMicroservice.Domain.Entities.Exam examDto)
        {
            if (examDto == null)
                return BadRequest("Exam payload is required");

            var query = new ClasseMicroservice.Application.Queries.GetClassByIdQuery(id);
            var classObj = await _getByIdHandler.HandleAsync(query);
            if (classObj == null)
                return NotFound();

            if (classObj.Exams == null)
                classObj.Exams = new List<ClasseMicroservice.Domain.Entities.Exam>();

            examDto.Id = string.IsNullOrWhiteSpace(examDto.Id) ? Guid.NewGuid().ToString() : examDto.Id;
            classObj.Exams.Add(examDto);

            var command = new ClasseMicroservice.Application.Commands.UpdateClassCommand(id, classObj);
            await _updateHandler.HandleAsync(command);

            return CreatedAtAction(nameof(GetExams), new { id = id }, examDto);
        }

        [HttpGet("{id}/exams")]
        [SwaggerOperation(
            Summary = "Listar provas da classe",
            Description = "Retorna todas as provas associadas a uma classe.",
            OperationId = "GetExams",
            Tags = new[] { "Exams" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExams(string id)
        {
            var query = new ClasseMicroservice.Application.Queries.GetClassByIdQuery(id);
            var classObj = await _getByIdHandler.HandleAsync(query);
            if (classObj == null)
                return NotFound();

            return Ok(classObj.Exams ?? new List<ClasseMicroservice.Domain.Entities.Exam>());
        }

        [HttpGet("{id}/exams/{examId}")]
        [SwaggerOperation(
            Summary = "Obter prova por ID",
            Description = "Retorna os dados de uma prova específica da classe.",
            OperationId = "GetExamById",
            Tags = new[] { "Exams" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExamById(string id, string examId)
        {
            var query = new ClasseMicroservice.Application.Queries.GetClassByIdQuery(id);
            var classObj = await _getByIdHandler.HandleAsync(query);
            if (classObj == null)
                return NotFound();

            var exam = classObj.Exams?.FirstOrDefault(e => e.Id == examId);
            if (exam == null)
                return NotFound();

            return Ok(exam);
        }

        [HttpPut("{id}/exams/{examId}")]
        [SwaggerOperation(
            Summary = "Atualizar prova",
            Description = "Atualiza integralmente uma prova da classe.",
            OperationId = "UpdateExam",
            Tags = new[] { "Exams" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateExam(string id, string examId, [FromBody] ClasseMicroservice.Domain.Entities.Exam examDto)
        {
            if (examDto == null)
                return BadRequest("Exam payload is required");

            var query = new ClasseMicroservice.Application.Queries.GetClassByIdQuery(id);
            var classObj = await _getByIdHandler.HandleAsync(query);
            if (classObj == null)
                return NotFound();

            var exam = classObj.Exams?.FirstOrDefault(e => e.Id == examId);
            if (exam == null)
                return NotFound();

            exam.Name = examDto.Name;
            exam.Date = examDto.Date;
            exam.Weight = examDto.Weight;

            var command = new ClasseMicroservice.Application.Commands.UpdateClassCommand(id, classObj);
            await _updateHandler.HandleAsync(command);
            return Ok(exam);
        }

        [HttpDelete("{id}/exams/{examId}")]
        [SwaggerOperation(
            Summary = "Excluir prova",
            Description = "Remove uma prova da classe pelo ID.",
            OperationId = "DeleteExam",
            Tags = new[] { "Exams" }
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteExam(string id, string examId)
        {
            var query = new ClasseMicroservice.Application.Queries.GetClassByIdQuery(id);
            var classObj = await _getByIdHandler.HandleAsync(query);
            if (classObj == null)
                return NotFound();

            var removed = classObj.Exams?.RemoveAll(e => e.Id == examId) > 0;
            if (!removed)
                return NotFound();

            var command = new ClasseMicroservice.Application.Commands.UpdateClassCommand(id, classObj);
            await _updateHandler.HandleAsync(command);
            return NoContent();
        }

        [HttpPatch("{id}/exams/{examId}")]
        [SwaggerOperation(
            Summary = "Atualização parcial da prova",
            Description = "Aplica alterações parciais em uma prova da classe.",
            OperationId = "PatchExam",
            Tags = new[] { "Exams" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PatchExam(string id, string examId, [FromBody] Dictionary<string, object> updates)
        {
            if (updates == null || updates.Count == 0)
                return BadRequest("Updates payload is required");

            var query = new ClasseMicroservice.Application.Queries.GetClassByIdQuery(id);
            var classObj = await _getByIdHandler.HandleAsync(query);
            if (classObj == null)
                return NotFound();

            var exam = classObj.Exams?.FirstOrDefault(e => e.Id == examId);
            if (exam == null)
                return NotFound();

            ApplyDictionaryToObject(exam, updates);

            var command = new ClasseMicroservice.Application.Commands.UpdateClassCommand(id, classObj);
            await _updateHandler.HandleAsync(command);
            return Ok(exam);
        }

        // --- Helpers ---

        private void ApplyDictionaryToObject(object target, Dictionary<string, object> updates)
        {
            if (target == null || updates == null) return;

            var targetType = target.GetType();
            foreach (var kv in updates)
            {
                var propName = kv.Key;
                var prop = targetType.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || !prop.CanWrite) continue;
                try
                {
                    var currentType = prop.PropertyType;

                    var targetTypeToConvert = Nullable.GetUnderlyingType(currentType) ?? currentType;

                    if (kv.Value == null)
                    {
                        prop.SetValue(target, null);
                        continue;
                    }

                    if (targetTypeToConvert.IsEnum)
                    {
                        var enumVal = Enum.Parse(targetTypeToConvert, kv.Value.ToString(), true);
                        prop.SetValue(target, enumVal);
                        continue;
                    }

                    if (!IsSimpleType(targetTypeToConvert) && kv.Value is System.Text.Json.JsonElement je)
                    {
                        var json = je.GetRawText();
                        var deserialized = System.Text.Json.JsonSerializer.Deserialize(json, currentType);
                        prop.SetValue(target, deserialized);
                        continue;
                    }

                    var converted = Convert.ChangeType(kv.Value, targetTypeToConvert);
                    prop.SetValue(target, converted);
                }
                catch
                {
                    continue;
                }
            }
        }

        private bool IsSimpleType(Type type)
        {
            return
                type.IsPrimitive ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(Guid);
        }
    }
}
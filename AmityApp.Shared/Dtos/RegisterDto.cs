using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmityApp.Shared.Dtos;

public record RegisterDto(string Name, string Email, string Password);
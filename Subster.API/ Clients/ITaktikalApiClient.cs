using Subster.Models.InputModels;
using Subster.Models.ResponseModels;

namespace Subster.API.Clients;

public interface ITaktikalApiClient
{
  Task<StartAuthResponseModel?> StartAsync(StartAuthInputModel dto);
  Task<PollResponseModel?> PollAsync (PollAuthInputModel dto);
}
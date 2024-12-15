using GotExplorer.BLL.DTOs;
using GotExplorer.BLL.Services.Results;

namespace GotExplorer.BLL.Services.Interfaces
{
    public interface IModel3DService
    {
        Task<ServiceResult<Model3dDTO>> GetModel3dAsync(int id);
        Task<ServiceResult> UploadModel3dAsync(UploadModel3dDTO uploadModel3dDTO);
        Task<ServiceResult> UpdateModel3dAsync(int id, UploadModel3dDTO uploadModel3dDTO);
        Task<ServiceResult> DeleteModel3dAsync(int id);
    }
}

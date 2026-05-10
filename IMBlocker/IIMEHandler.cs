using Vintagestory.API.Client;

namespace IMBlocker;

public interface IIMEHandler
{
	void Initialize(ICoreClientAPI api);
	void EnableIME();
	void DisableIME();
}
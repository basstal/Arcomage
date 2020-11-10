using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class AddressableUtility
{
    public static async Task InitByNameOrLabel<T>(string assetNameOrLabel, List<T> loadedObjs)
        where T : Object
    {
        var locations = await Addressables.LoadResourceLocationsAsync(assetNameOrLabel).Task;
    
        await CreateAssetsThenUpdateCollection(locations, loadedObjs);
    }

    public static async Task IniByLoadedAddress<T>(IList<IResourceLocation> loadedLocations, List<T> loadedObjs)
        where T : Object
    {
        await CreateAssetsThenUpdateCollection(loadedLocations, loadedObjs);
    }

    private static async Task CreateAssetsThenUpdateCollection<T>(IList<IResourceLocation> locations, List<T> loadedObjs)
        where T: Object
    {
        foreach (var location in locations)
            loadedObjs.Add(await Addressables.LoadAssetAsync<T>(location).Task);
    }
}
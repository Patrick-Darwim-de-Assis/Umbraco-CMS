using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{

    /// <summary>
    /// Represents a factory to create <see cref="MenuItemCollection"/>.
    /// </summary>
    public interface IMenuItemCollectionFactory
    {
        /// <summary>
        /// Creates an empty <see cref="MenuItemCollection"/>.
        /// </summary>
        /// <returns>An empty <see cref="MenuItemCollection"/>.</returns>
        MenuItemCollection Create();
    }
}

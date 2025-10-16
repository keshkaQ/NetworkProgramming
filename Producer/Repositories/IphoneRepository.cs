using Producer.Models;
using System.Collections.ObjectModel;

namespace Producer.Repositories
{
    public class IphoneRepository
    {
        private readonly DBContext _dbContext;

        public IphoneRepository(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ObservableCollection<iPhoneModel> LoadIphoneModels()
        {
            var alliPhones = _dbContext.Iphones.ToList();
            var iPhoneModels = new ObservableCollection<iPhoneModel>();
            var grouped = alliPhones.GroupBy(iphone => iphone.BaseModelName);

            foreach (var group in grouped.OrderByDescending(g => g.Key))
            {
                var model = new iPhoneModel
                {
                    ModelName = group.Key,
                    ColorVariants = new ObservableCollection<Iphone>(group.OrderBy(i => i.Color).ThenBy(i => i.BasePrice))
                };
                iPhoneModels.Add(model);
            }
            return iPhoneModels;
        }
    }
}
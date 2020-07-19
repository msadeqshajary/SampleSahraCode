using Infrastructure;
using Infrastructure.Inv;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace InventoryModule.ViewModels
{
    class MaterialViewModel : BindableBase
    {
        public ObservableCollection<InvMaterials> Materials { get; set; }
        private InvMaterials material;
        public InvMaterials Material { get => material; set => SetProperty(ref material, value); }

        public ObservableCollection<InvProducts> Products { get; set; }

        private string materialName;
        public string MaterialName { get => materialName; set=> SetProperty(ref materialName, value); }

        private string productName;
        public string ProductName { get => productName; set => SetProperty(ref productName, value); }

        IInventoryService invCl = new InventoryService();

        public DelegateCommand AddMaterialCommand { get; set; }
        public DelegateCommand AddProductCommand { get; set; }

        public MaterialViewModel()
        {
            Materials = new ObservableCollection<InvMaterials>();
            Products = new ObservableCollection<InvProducts>();
            AddMaterialCommand = new DelegateCommand(AddMaterial);
            AddProductCommand = new DelegateCommand(AddProduct);

            PropertyChanged += MaterialViewModel_PropertyChanged;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                foreach(var item in invCl.GetMaterials())
                {
                    Materials.Add(item);
                }
            };

            worker.RunWorkerAsync();
        }

        private void AddProduct()
        {
            if (string.IsNullOrEmpty(ProductName))
            {
                MessageBox.Show("نام محصول انتخاب نشده است", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(Products.Any(a => a.Name == ProductName))
            {
                MessageBox.Show("نام محصول نمی تواند تکراری باشد", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Material == null)
            {
                MessageBox.Show("متریال انتخاب نشده است", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            InvProducts pro = new InvProducts
            {
                InvMaterialId = Material.Id,
                Name = ProductName,
                Material = Material
            };

            pro.Id = invCl.InsertProduct(pro);
            Products.Add(pro);
            ProductName = string.Empty;
        }

        private void MaterialViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Material")
            {
                if(Material != null)
                {
                    Products.Clear();
                    foreach(var item in invCl.GetProductsByMaterial(Material.Id))
                    {
                        item.Material = Material;
                        Products.Add(item);
                    }
                }
            }
        }

        private void AddMaterial()
        {
            if (string.IsNullOrEmpty(MaterialName))
            {
                MessageBox.Show("لطفا نام متریال را وارد کنید", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(Materials.Any(a => a.Name == MaterialName))
            {
                MessageBox.Show("نام متریال نمی تواند تکراری باشد", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            InvMaterials mat = new InvMaterials
            {
                Name = MaterialName
            };
            mat.Id = invCl.InsertMaterial(mat);
            Materials.Add(mat);
            MaterialName = string.Empty;
        }
    }
}

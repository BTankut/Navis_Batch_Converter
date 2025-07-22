# Güncelleme - Duplicate ConversionJob Sorunu Çözüldü

## Sorun
MainViewModel.cs dosyasında iki farklı ConversionJob sınıfı vardı:
1. `RevitNavisworksAutomation.Models.ConversionJob` (doğru olan)
2. `RevitNavisworksAutomation.UI.ViewModels.ConversionJob` (MainViewModel sonunda duplicate)

## Çözüm
MainViewModel.cs dosyasının sonundaki duplicate ConversionJob sınıfı silindi (satır 710-770).

## Yapılması Gerekenler
1. Visual Studio'da **Build > Rebuild Solution**
2. F5 ile uygulamayı çalıştırın

## Beklenen Sonuç
- Progress dialog artık içerik gösterecek
- Dosya status'ları düzgün güncellenecek
- "Pending" durumu "Processing" ve "Completed" olarak değişecek
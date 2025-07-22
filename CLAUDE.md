# Navis Batch Converter - Proje Yol Haritası

## Proje Genel Bakış
Revit (.rvt) dosyalarını toplu olarak Navisworks (.nwc/.nwd) formatına dönüştüren, modern WPF arayüzlü bir otomasyon aracı.

## Teknoloji Stack
- **UI Framework**: WPF + Material Design + MahApps.Metro
- **Backend**: C# (.NET Framework 4.7.2+)
- **Otomasyon**: RevitBatchProcessor (CLI mode default)
- **Script**: PowerShell
- **Hedef Versiyonlar**: Revit/Navisworks 2021-2022

## Yapılanlar ✅

### 1. Proje Planlama ve Dokümantasyon
- [x] CLAUDE.md dosyası oluşturuldu
- [x] Proje yol haritası hazırlandı
- [x] Todo listesi sistemi kuruldu

## Yapılacaklar 📋

### 2. Proje Altyapısı
- [ ] Dizin yapısını oluştur
  - [ ] src/UI klasörü (XAML ve ViewModels)
  - [ ] src/Core klasörü (İş mantığı)
  - [ ] src/Models klasörü (Veri modelleri)
  - [ ] scripts klasörü (PowerShell)
  - [ ] Resources klasörü (İkonlar, resimler)
  - [ ] logs ve output klasörleri

### 3. WPF Uygulama Temeli
- [ ] .NET Framework projesi oluştur
- [ ] NuGet paketlerini yükle:
  - [ ] MaterialDesignThemes
  - [ ] MahApps.Metro
  - [ ] Microsoft.Xaml.Behaviors.Wpf
  - [ ] Prism.Core
  - [ ] Newtonsoft.Json
- [ ] App.xaml ve App.xaml.cs
- [ ] Tema ve stil yapılandırması

### 4. Ana Kullanıcı Arayüzü
- [ ] MainWindow.xaml - Modern Material Design arayüz
- [ ] MainViewModel.cs - MVVM pattern
- [ ] Özel kontroller:
  - [ ] FileListControl - Dosya listesi görünümü
  - [ ] ProgressControl - İlerleme göstergesi
  - [ ] SettingsPanel - Ayarlar paneli

### 5. Core İşlevsellik
- [ ] RevitExportTask.cs - Ana dönüştürme mantığı
- [ ] WorksetManager.cs - Workset filtreleme
- [ ] ViewSelector.cs - View filtreleme
- [ ] Logger.cs - Loglama sistemi
- [ ] ErrorHandler.cs - Hata yönetimi

### 6. Veri Modelleri
- [ ] ConversionJob.cs - Dönüştürme işi modeli
- [ ] ConversionSettings.cs - Ayarlar modeli
- [ ] ConversionResult.cs - Sonuç modeli

### 7. PowerShell Entegrasyonu
- [ ] RunBatchConverter.ps1 - Ana orchestrator
- [ ] Config.json - Yapılandırma dosyası
- [ ] Install.ps1 - Kurulum scripti

### 8. RevitBatchProcessor Entegrasyonu
- [ ] CLI mode entegrasyonu (varsayılan)
- [ ] Advanced Mode butonu (Native GUI erişimi)
- [ ] Arka planda çalışma (GUI göstermeden)

### 9. Özellikler
- [ ] Drag & drop dosya desteği
- [ ] Gerçek zamanlı ilerleme takibi
- [ ] Workset filtreleme (kullanıcı seçilebilir)
- [ ] 3D View filtreleme ("navis_view" içeren)
- [ ] NWC'den NWD'ye birleştirme
- [ ] Hata kurtarma ve yeniden deneme
- [ ] Ayarların kalıcılığı

### 10. Test ve Doğrulama
- [ ] UI yanıt verme testi
- [ ] Drag & drop fonksiyonelliği
- [ ] İlerleme göstergeleri
- [ ] CLI mode'da GUI görünmemesi
- [ ] Workset ve view filtreleme
- [ ] Hata kurtarma mekanizmaları

### 11. Dokümantasyon
- [ ] README.md oluştur
- [ ] Kurulum talimatları
- [ ] Kullanım kılavuzu
- [ ] Sorun giderme rehberi

### 12. Git ve Versiyon Kontrolü
- [ ] Git repository başlat
- [ ] .gitignore dosyası
- [ ] İlk commit
- [ ] GitHub'a push

## Notlar ve Karşılaşılan Sorunlar 📝

### Genel Notlar
- RevitBatchProcessor varsayılan olarak CLI modunda çalışacak (GUI göstermeyecek)
- Advanced Mode butonu ile native GUI'ye erişim sağlanacak
- Material Design ve MahApps.Metro ile modern, profesyonel görünüm
- MVVM pattern kullanılacak

### Kritik Noktalar
- RevitBatchProcessor'ın doğru kurulumu gerekli
- Revit API DLL referansları doğru ayarlanmalı
- .NET Framework 4.7.2+ gerekli
- Windows platformuna özel

## İletişim ve Raporlama
- Major kararlar için kullanıcıya danışılacak
- Her adımda ilerleme güncellemesi
- Sorunlar anında raporlanacak

## Sonraki Adım
Proje dizin yapısını oluşturmak ve temel WPF uygulamasını kurmak.

---
*Son güncelleme: 2025-07-22 14:31:25*
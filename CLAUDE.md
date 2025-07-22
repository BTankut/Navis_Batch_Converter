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
- [x] README.md oluşturuldu

### 2. Proje Altyapısı
- [x] Dizin yapısı oluşturuldu
- [x] Git repository başlatıldı
- [x] .gitignore eklendi
- [x] İlk commit yapıldı

## Yapılacaklar 📋

### 3. WPF Uygulama Temeli
- [x] .NET Framework projesi oluştur (.csproj)
- [x] App.config dosyası oluşturuldu
- [x] App.xaml ve App.xaml.cs
- [x] Tema ve stil yapılandırması (Material Design + MahApps)
- [x] Properties klasörü ve AssemblyInfo.cs
- [ ] NuGet paketlerini yükle:
  - [ ] MaterialDesignThemes
  - [ ] MahApps.Metro
  - [ ] Microsoft.Xaml.Behaviors.Wpf
  - [ ] Prism.Core
  - [ ] Newtonsoft.Json

### 4. Ana Kullanıcı Arayüzü
- [x] MainWindow.xaml - Modern Material Design arayüz
- [x] MainWindow.xaml.cs - Code-behind
- [x] MainViewModel.cs - MVVM pattern
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

## Mevcut Durum
Proje altyapısı hazır, WPF ana pencere ve ViewModel implementasyonu tamamlandı. Şu anda uygulama temel arayüz ile açılabilir durumda. Core işlevsellik ve RevitBatchProcessor entegrasyonu üzerinde çalışılıyor.

### Karşılaşılan Sorunlar ve Çözümler
- Henüz kritik bir sorun yaşanmadı
- NuGet paketleri henüz yüklenmedi, Visual Studio'da restore edilmesi gerekiyor

---
*Son güncelleme: 2025-07-22 14:45:10*
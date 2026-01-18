$cert = New-SelfSignedCertificate -Type Custom -Subject "CN=8136FD48-8D5A-4FB2-8DF9-A606C867768F" -KeyUsage DigitalSignature -FriendlyName "Types.WinUI Store Cert" -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
$password = ConvertTo-SecureString -String "password" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "D:\Users\Shomn\OneDrive - MSFT\Downloads\Types.Src\Types\Types.WinUI\Types.WinUI_StoreKey.pfx" -Password $password

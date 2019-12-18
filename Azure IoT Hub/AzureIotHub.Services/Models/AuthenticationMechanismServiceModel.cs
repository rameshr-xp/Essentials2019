

using Microsoft.Azure.Devices;
using System;

namespace AzureIotHub.Services.Models
{
    public class AuthenticationMechanismServiceModel
    {
        public AuthenticationMechanismServiceModel()
        {
        }

        internal AuthenticationMechanismServiceModel(AuthenticationMechanism azureModel)
        {
            switch (azureModel.Type)
            {
                case Microsoft.Azure.Devices.AuthenticationType.Sas:
                    PrimaryKey = azureModel.SymmetricKey.PrimaryKey;
                    SecondaryKey = azureModel.SymmetricKey.SecondaryKey;
                    break;
                case Microsoft.Azure.Devices.AuthenticationType.SelfSigned:
                    AuthenticationType = AuthenticationType.SelfSigned;
                    PrimaryThumbprint = azureModel.X509Thumbprint.PrimaryThumbprint;
                    SecondaryThumbprint = azureModel.X509Thumbprint.SecondaryThumbprint;
                    break;
                case Microsoft.Azure.Devices.AuthenticationType.CertificateAuthority:
                    AuthenticationType = AuthenticationType.CertificateAuthority;
                    PrimaryThumbprint = azureModel.X509Thumbprint.PrimaryThumbprint;
                    SecondaryThumbprint = azureModel.X509Thumbprint.SecondaryThumbprint;
                    break;
                default:
                    throw new ArgumentException("Not supported authentcation type");
            }
        }

        public string PrimaryKey { get; set; }

        public string SecondaryKey { get; set; }

        public string PrimaryThumbprint { get; set; }

        public string SecondaryThumbprint { get; set; }

        public AuthenticationType AuthenticationType { get; set; }

        public AuthenticationMechanism ToAzureModel()
        {
            AuthenticationMechanism auth = new AuthenticationMechanism();

            switch (AuthenticationType)
            {
                case AuthenticationType.Sas:
                    {
                        auth.SymmetricKey = new SymmetricKey()
                        {
                            PrimaryKey = PrimaryKey,
                            SecondaryKey = SecondaryKey
                        };

                        auth.Type = Microsoft.Azure.Devices.AuthenticationType.Sas;

                        break;
                    }
                case AuthenticationType.SelfSigned:
                    {
                        auth.X509Thumbprint = new X509Thumbprint()
                        {
                            PrimaryThumbprint = PrimaryThumbprint,
                            SecondaryThumbprint = SecondaryThumbprint
                        };

                        auth.Type = Microsoft.Azure.Devices.AuthenticationType.SelfSigned;

                        break;
                    }
                case AuthenticationType.CertificateAuthority:
                    {
                        auth.X509Thumbprint = new X509Thumbprint()
                        {
                            PrimaryThumbprint = PrimaryThumbprint,
                            SecondaryThumbprint = SecondaryThumbprint
                        };

                        auth.Type = Microsoft.Azure.Devices.AuthenticationType.CertificateAuthority;

                        break;
                    }
                default:
                    throw new ArgumentException("Not supported authentcation type");
            }

            return auth;
        }
    }

    public enum AuthenticationType
    {
        //
        // Summary:
        //     Shared Access Key
        Sas = 0,

        //
        // Summary:
        //     Self-signed certificate
        SelfSigned = 1,

        //
        // Summary:
        //     Certificate Authority
        CertificateAuthority = 2
    }
}

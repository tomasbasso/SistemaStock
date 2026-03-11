using System;

namespace Sistema_de_Stock.Models
{
    public class Categoria
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
    }

    public class Producto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public int Stock { get; set; } = 0;
        public int StockMinimo { get; set; } = 0;
        public decimal Price { get; set; } = 0;
    }

    public class Cliente
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class CuentaCorriente
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClienteId { get; set; }
        public decimal Balance { get; set; } = 0; // Positive means debt to the store, negative means store owes client
    }

    public enum TipoMovimiento
    {
        Ingreso,
        Egreso
    }

    public class MovimientoFinanciero
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public TipoMovimiento Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
        public Guid? VentaId { get; set; } // Nullable, linked if generated from a sale
    }

    public class Venta
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int NumeroVenta { get; set; } // Auto-increment style assigned securely before save
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public Guid? ClienteId { get; set; }
        public bool IsFiado { get; set; } = false; // Add parameter to know if it was cash or credit
    }

    public class VentaDetalle
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid VentaId { get; set; }
        public Guid ProductoId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class ConfiguracionApp
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string NombreNegocio { get; set; } = "Mi Negocio";
        public string Moneda { get; set; } = "ARS";
        public string DireccionNegocio { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }
}

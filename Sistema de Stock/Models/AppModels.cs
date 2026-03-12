using System;
using System.ComponentModel.DataAnnotations;

namespace Sistema_de_Stock.Models
{
    public class Categoria
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
        public string Name { get; set; } = string.Empty;
    }

    public class Producto
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [MaxLength(200, ErrorMessage = "El nombre no puede superar 200 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "El SKU no puede superar 50 caracteres.")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una categoría.")]
        public Guid CategoryId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; } = 0;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
        public int StockMinimo { get; set; } = 0;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
        public decimal Price { get; set; } = 0;

        [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
        [MaxLength(20, ErrorMessage = "La unidad de medida no puede superar 20 caracteres.")]
        public string UnidadMedida { get; set; } = "u.";

        [MaxLength(100, ErrorMessage = "La ubicación no puede superar 100 caracteres.")]
        public string Ubicacion { get; set; } = string.Empty;
    }

    public class Cliente
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        [MaxLength(200, ErrorMessage = "El nombre no puede superar 200 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "El teléfono no puede superar 50 caracteres.")]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(300, ErrorMessage = "La dirección no puede superar 300 caracteres.")]
        public string Address { get; set; } = string.Empty;
    }

    public class CuentaCorriente
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ClienteId { get; set; }

        public decimal Balance { get; set; } = 0; // Positivo = deuda del cliente; negativo = a favor del cliente
    }

    public enum TipoMovimiento
    {
        Ingreso,
        Egreso
    }

    public class MovimientoFinanciero
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public TipoMovimiento Type { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [MaxLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
        public string Description { get; set; } = string.Empty;

        public Guid? VentaId { get; set; }
    }

    public class Venta
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int NumeroVenta { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public Guid? ClienteId { get; set; }
        public bool IsFiado { get; set; } = false;
    }

    public class VentaDetalle
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid VentaId { get; set; }

        [Required]
        public Guid ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }

    public class Presupuesto
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int NumeroPresupuesto { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime? FechaVencimiento { get; set; }
        public decimal Total { get; set; }
        public Guid? ClienteId { get; set; }

        [MaxLength(500)]
        public string Notas { get; set; } = string.Empty;
    }

    public class PresupuestoDetalle
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PresupuestoId { get; set; }

        [Required]
        public Guid ProductoId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }

    public class ConfiguracionApp
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "El nombre del negocio es obligatorio.")]
        [MaxLength(150, ErrorMessage = "El nombre del negocio no puede superar 150 caracteres.")]
        public string NombreNegocio { get; set; } = "Mi Negocio";

        [MaxLength(10, ErrorMessage = "La moneda no puede superar 10 caracteres.")]
        public string Moneda { get; set; } = "ARS";

        [MaxLength(300)]
        public string DireccionNegocio { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Telefono { get; set; } = string.Empty;
    }
}
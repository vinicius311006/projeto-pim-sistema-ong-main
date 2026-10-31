using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VestindoSonhos.Models;

public partial class VestindosonhosdbContext : DbContext
{
    public VestindosonhosdbContext()
    {
    }

    public VestindosonhosdbContext(DbContextOptions<VestindosonhosdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Beneficiario> Beneficiario { get; set; }

    public virtual DbSet<Cargo> Cargo { get; set; }

    public virtual DbSet<Carteira> Carteira { get; set; }

    public virtual DbSet<Categoria> Categoria { get; set; }

    public virtual DbSet<Dependente> Dependente { get; set; }

    public virtual DbSet<Evento> Evento { get; set; }

    public virtual DbSet<Produto> Produto { get; set; }

    public virtual DbSet<TiposAtivo> TiposAtivo { get; set; }

    public virtual DbSet<Transacao> Transacao { get; set; }

    public virtual DbSet<TransacaoFinanceira> TransacaoFinanceira { get; set; }

    public virtual DbSet<TransacaoInventario> TransacaoInventario { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    public virtual DbSet<Voluntario> Voluntario { get; set; }

    public virtual DbSet<VoluntarioAtribuido> VoluntarioAtribuido { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connStr = "Server=localhost;Port=3306;Database=VESTINDOSONHOSDB;User=root;Password=root;";
            optionsBuilder.UseMySQL(connStr);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Beneficiario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Benefici__3213E83F08C6E0D2");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Endereco)
                .HasMaxLength(60)
                .HasColumnName("endereco");
            entity.Property(e => e.Nome)
                .HasMaxLength(80)
                .HasColumnName("nome");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.Situacao)
                .HasMaxLength(20)
                .HasColumnName("situacao");
            entity.Property(e => e.Telefone)
                .HasMaxLength(15)
                .HasColumnName("telefone");
        });

        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.ToTable("Cargos");
            entity.HasKey(e => e.Id).HasName("PK__Cargo__3213E83F81CDD3A4");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<Carteira>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Carteira__3213E83FA93BC5A4");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome)
                .HasMaxLength(30)
                .HasColumnName("nome");
            entity.Property(e => e.Saldo)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("saldo");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3213E83F5FA07FA2");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome)
                .HasMaxLength(15)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<Dependente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Dependen__3213E83FDFA9E502");

            entity.Property(e => e.Sexo).IsFixedLength();

            entity.HasOne(d => d.IdResponsavelNavigation).WithMany(p => p.Dependente).HasConstraintName("FK_Dependente_Beneficiario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdResponsavel).HasColumnName("id_responsavel");
            entity.Property(e => e.Idade).HasColumnName("idade");
            entity.Property(e => e.Nome)
                .HasMaxLength(60)
                .HasColumnName("nome");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.Parentesco)
                .HasMaxLength(20)
                .HasColumnName("parentesco");
            entity.Property(e => e.Sexo)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("sexo");

            entity.HasOne(d => d.IdResponsavelNavigation).WithMany(p => p.Dependente)
                .HasForeignKey(d => d.IdResponsavel)
                .HasConstraintName("FK_Dependente_Beneficiario");
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.ToTable("Eventos");
            entity.HasKey(e => e.Id).HasName("PK__Evento__3213E83F27871B31");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.data_fim).HasColumnName("data_fim");
            entity.Property(e => e.data_inicio).HasColumnName("data_inicio");
            entity.Property(e => e.Local)
                .HasMaxLength(70)
                .HasColumnName("local");
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .HasColumnName("nome");
            entity.Property(e => e.Observacao).HasColumnName("observacao");
            entity.Property(e => e.Responsavel).HasColumnName("responsavel");

            entity.HasOne(d => d.ResponsavelNavigation).WithMany(p => p.Evento)
                .HasForeignKey(d => d.Responsavel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Evento_Usuario");
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Produto__3213E83F81046C04");

            entity.HasOne(d => d.CategoriaNavigation).WithMany(p => p.Produto).HasConstraintName("FK_Produto_Tipo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Categoria).HasColumnName("categoria");
            entity.Property(e => e.Cor)
                .HasMaxLength(15)
                .HasColumnName("cor");
            entity.Property(e => e.Descricao)
                .HasMaxLength(255)
                .HasColumnName("descricao");
            entity.Property(e => e.Nome)
                .HasMaxLength(60)
                .HasColumnName("nome");

            entity.HasOne(d => d.CategoriaNavigation).WithMany(p => p.Produto)
                .HasForeignKey(d => d.Categoria)
                .HasConstraintName("FK_Produto_Tipo");
        });

        modelBuilder.Entity<TiposAtivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tipos_at__3213E83F8EF166EC");

            entity.ToTable("TiposAtivo");

            entity.HasIndex(e => e.Categoria, "UQ__Tipos_at__1179412FC33A7C31").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Categoria)
                .HasMaxLength(15)
                .HasColumnName("categoria");
        });

        modelBuilder.Entity<Transacao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transaca__3213E83F82282BCA");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Categoria).HasColumnName("categoria");
            entity.Property(e => e.Hora).HasColumnName("hora");
            entity.Property(e => e.Observacao)
                .HasMaxLength(255)
                .HasColumnName("observacao");
            entity.Property(e => e.Razao)
                .HasMaxLength(255)
                .HasColumnName("razao");
            entity.Property(e => e.Usuario).HasColumnName("usuario");

            entity.HasOne(d => d.CategoriaNavigation).WithMany(p => p.Transacao)
                .HasForeignKey(d => d.Categoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transacao_Categoria");

            entity.HasOne(d => d.UsuarioNavigation).WithMany(p => p.Transacao)
                .HasForeignKey(d => d.Usuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transacao_Usuario");
        });

        modelBuilder.Entity<TransacaoFinanceira>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transaca__3213E83FAE6BE065");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Carteira).HasColumnName("carteira");
            entity.Property(e => e.Valor)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("valor");

            entity.HasOne(d => d.CarteiraNavigation).WithMany(p => p.TransacaoFinanceira)
                .HasForeignKey(d => d.Carteira)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TF_Carteira");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.TransacaoFinanceira)
                .HasForeignKey<TransacaoFinanceira>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TF_Transacao");
        });

        modelBuilder.Entity<TransacaoInventario>(entity =>
        {
            entity.HasKey(e => new { e.ProdutoId, e.TransacaoId }).HasName("PK__Transaca__CA3F376987ED0A0E");

            entity.HasOne(d => d.BeneficiarioNavigation).WithMany(p => p.TransacaoInventario).HasConstraintName("FK_TransacaoInventario_Beneficiario");

            entity.Property(e => e.ProdutoId).HasColumnName("produto_id");
            entity.Property(e => e.TransacaoId).HasColumnName("transacao_id");
            entity.Property(e => e.Beneficiario).HasColumnName("beneficiario");
            entity.Property(e => e.Quantidade).HasColumnName("quantidade");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3213E83FF41E77E6");

            entity.HasIndex(e => e.Cpf, "UQ__Usuario__D836E71F0996A7A1").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cargo).HasColumnName("cargo");
            entity.Property(e => e.Cpf)
                .HasMaxLength(11)
                .HasColumnName("cpf");
            entity.Property(e => e.DataNascimento).HasColumnName("data_nascimento");
            entity.Property(e => e.Email)
                .HasMaxLength(40)
                .HasColumnName("email");
            entity.Property(e => e.Endereco)
                .HasMaxLength(80)
                .HasColumnName("endereco");
            entity.Property(e => e.Nome)
                .HasMaxLength(50)
                .HasColumnName("nome");
            entity.Property(e => e.Senha)
                .HasMaxLength(255)
                .HasColumnName("senha");
            entity.Property(e => e.Telefone)
                .HasMaxLength(14)
                .HasColumnName("telefone");
            entity.Property(e => e.ExigirTrocaSenha)
                .HasColumnName("exigir_troca_senha")
                .HasDefaultValue(false);

            entity.HasOne(d => d.CargoNavigation).WithMany(p => p.Usuario)
                .HasForeignKey(d => d.Cargo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Cargo");
        });

        modelBuilder.Entity<Voluntario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Voluntar__3213E83F4D46C84D");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cpf)
                .HasMaxLength(11)
                .HasColumnName("cpf");
            entity.Property(e => e.Email)
                .HasMaxLength(40)
                .HasColumnName("email");
            entity.Property(e => e.Endereco)
                .HasMaxLength(50)
                .HasColumnName("endereco");
            entity.Property(e => e.Nome)
                .HasMaxLength(60)
                .HasColumnName("nome");
            entity.Property(e => e.Telefone)
                .HasMaxLength(14)
                .HasColumnName("telefone");
        });

        modelBuilder.Entity<VoluntarioAtribuido>(entity =>
        {
            entity.HasKey(e => new { e.Evento, e.Voluntario }).HasName("PK__Voluntar__1439D9B41A87BE6E");

            entity.Property(e => e.DataAdicao).HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Evento).HasColumnName("evento");
            entity.Property(e => e.Voluntario).HasColumnName("voluntario");
            entity.Property(e => e.DataAdicao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("data_adicao");

            entity.HasOne(d => d.EventoNavigation).WithMany(p => p.VoluntarioAtribuido)
                .HasForeignKey(d => d.Evento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VA_Evento");

            entity.HasOne(d => d.VoluntarioNavigation).WithMany(p => p.VoluntarioAtribuido)
                .HasForeignKey(d => d.Voluntario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VA_Voluntario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

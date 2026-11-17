package br.com.bairroemdia.repository;

import br.com.bairroemdia.domain.Analise;
import java.util.List;
import java.util.UUID;
import org.springframework.data.jpa.repository.JpaRepository;

public interface AnaliseRepository extends JpaRepository<Analise, UUID> {
    List<Analise> findTop20ByOrderByGeradaEmDescIdDesc();
}

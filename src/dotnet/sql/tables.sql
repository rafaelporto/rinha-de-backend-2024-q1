CREATE TABLE IF NOT EXISTS contas (
    id INT primary key AUTO_INCREMENT,
    content VARCHAR(2500)
);

CREATE TABLE IF NOT EXISTS transacoes (
    id INT primary key AUTO_INCREMENT,
    conta_id INT,
    content VARCHAR(1024),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

ALTER TABLE transacoes
ADD CONSTRAINT fk_conta_id
FOREIGN KEY (conta_id)
REFERENCES contas(id);

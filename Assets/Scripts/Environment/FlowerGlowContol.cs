using UnityEngine;

namespace Game.Environment
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FlowerGlowContol : MonoBehaviour
    {
        [SerializeField]private Texture2D glowTex;
        [SerializeField]private Color glowColor;
        [SerializeField]private float glowSpeed = 0.7f;
        private SpriteRenderer sr;
        private MaterialPropertyBlock block;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            block = new MaterialPropertyBlock();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            UpdateGlow();
        }

        public void UpdateGlow()
        {
            sr.GetPropertyBlock(block);    
            block.SetTexture("_GlowTex", glowTex);
            block.SetColor("_GlowColor", glowColor); 
            sr.SetPropertyBlock(block);
        }

        // Update is called once per frame
        void Update()
        {
            //flower breath effect
            glowColor = Color.Lerp(Color.magenta, Color.gray, Mathf.PingPong(Time.time * glowSpeed, 1));
            UpdateGlow();
        }
    }
}
